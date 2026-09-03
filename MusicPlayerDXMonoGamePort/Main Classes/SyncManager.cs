using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EzAuth.Interfaces;
using EzAuth.Keycloak;
using MusicPlayerDXMonoGamePort.Persistence.Database;
using MusicPlayerSyncInterface;
using MusicPlayerSyncInterface.DTOs;
using MusicPlayerSyncInterface.DTOs.Composites;
using Newtonsoft.Json;
using Persistence;

namespace MusicPlayerDXMonoGamePort.Main_Classes;

public static class SyncManager
{
    static HttpClient? _httpClient = new();
    static IEzAuthHttpClient client = null;
    static EzAuthAddress authBackendAddress = null;
    public static string State { get => state; private set { OnStateChanged?.Invoke(value); state = value; } }
    private static string state = "";
    public static Action<string>? OnStateChanged = null;
    const string ROUTE_VERSION_PREFIX = "/v1";
    const string SONG_LIBRARY_CONFIG_FILE_NAME = ".song-library.music-player-config";

    /// <summary>
    /// The song library migrations that came with the last successful pull. Used to apply pending
    /// migrations (e.g. song file renames) to the local song library.
    /// </summary>
    public static SongLibraryMigration[] LastPulledMigrations { get; private set; } = [];

    /// <summary>
    /// The user id of the account the last successful pull was made for. The song library config file
    /// records which account a song library belongs to, so migrations are only applied to a library
    /// when its recorded account matches the account that is pulling.
    /// </summary>
    public static string? LastPulledUserId { get; private set; } = null;

    static string? songLibraryOwnerWarning = null;

    /// <summary>
    /// Returns the last recorded song library account warning (see <see cref="WriteSongLibraryMigrationState"/>) and clears it.
    /// UI code should show it to the user (e.g. a MessageBox) so misconfigured libraries are noticed.
    /// </summary>
    public static string? TakeSongLibraryOwnerWarning()
    {
        string? warning = songLibraryOwnerWarning;
        songLibraryOwnerWarning = null;
        return warning;
    }

    static SyncManager()
    {
        Init();
    }

    public static void Init(string? password = null, bool TryCallApiInit = false, bool RetryUnsyncedEntries = true)
    {
        try
        {
            authBackendAddress = GetAuthBackendAddress(Config.Data.SyncServerHost);
            client = new KeyCloakHttpClient(authBackendAddress, authBackendRefreshToken =>
            {
                Config.Data.AuthBackendRefreshToken = authBackendRefreshToken;
                Config.Save();
            }, Config.Data.AuthBackendRefreshToken, _httpClient);

            if (password != null)
                client.Login(Config.Data.SyncServerUsername, password);
        }
        catch (Exception ex)
        {
            State = $"SyncManager Init failed: {ex.Message}";
            return;
        }

        // Init
        try
        {
            if (TryCallApiInit)
            {
                using var songDbContext = new SongDbContext();
                var sendObjString = JsonConvert.SerializeObject(new SyncInitRequest(songDbContext.UpvotedSongs.ToArray(), songDbContext.SongHistoryEntries.ToArray()), Formatting.Indented);
                var sendContent = new StringContent(sendObjString, Encoding.UTF8, "application/json");
                var res = client.PostAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/init", sendContent).Result;
                State = $"Init {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
            }
        }
        catch (Exception ex)
        {
            State = $"API Init failed: {ex.Message}";
        }

        // Retry unsynced entries
        if (RetryUnsyncedEntries)
        {
            using var songDbContext = new SongDbContext();
            foreach (var unsyncedData in songDbContext.NotYetSyncedData.ToArray())
            {
                try
                {
                    var sendContent = new StringContent(unsyncedData.Body, Encoding.UTF8, "application/json");
                    HttpResponseMessage res;
                    if (unsyncedData.Endpoint == "/sync/volume")
                        res = client.PutAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}{unsyncedData.Endpoint}", sendContent).Result;
                    else
                        res = client.PostAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}{unsyncedData.Endpoint}", sendContent).Result;

                    Console.WriteLine($"Synced data for endpoint {unsyncedData.Endpoint}: {res.StatusCode}, {unsyncedData.Body}");

                    if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        songDbContext.NotYetSyncedData.Remove(unsyncedData);
                        songDbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    State = $"API Retry Unsynced Entries failed: {ex.Message}";
                    Console.WriteLine($"API Retry Unsynced Entries failed for endpoint {unsyncedData.Endpoint}: {ex.Message}, {unsyncedData.Body}");
                }
            }
        }
    }

    public static EzAuthAddress GetAuthBackendAddress(string syncServerHost)
    {
        var res = _httpClient.GetAsync($"{syncServerHost}{ROUTE_VERSION_PREFIX}/authBackend").Result;
        var content = res.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<EzAuthAddress>(content);
    }

    public static string GetAccountRegistrationAddress() => client.GetAccountRegistrationAddress();

    /// <summary>
    /// Pulls the latest data from the sync server and writes it into the local database.
    /// Before anything is written, the configured song library is checked: if it is registered for a
    /// different account than the one the pull is made for, the pull is aborted (nothing is synced, the
    /// database and the library state file stay untouched) and a warning is recorded, which the UI should
    /// surface to the user. The pull can then be retried with AdoptSongLibraryOnMismatch = true once the
    /// user explicitly agreed to take the library over for the current account.
    /// </summary>
    public static void Pull(bool AdoptSongLibraryOnMismatch = false)
    {
        try
        {
            var res = client.GetStringAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/pull").Result;
            var pulledData = JsonConvert.DeserializeObject<SyncPullResponse>(res);

            if (pulledData == null)
                throw new Exception("Pulled data was null!");
            if (pulledData.Songs.Count() == 0 || pulledData.HistoryEntries.Count() == 0)
                throw new Exception("Pulled data was empty!");

            string authedUserId = pulledData.User?.UserId ?? "";

            // Account check BEFORE any local side effects: if the configured song library is registered for
            // another account, stop here so the other accounts data is not written over the local database
            // and the library state is not touched before the user had a chance to decide what to do.
            bool libraryHasOtherOwner = false;
            if (authedUserId != "" && !string.IsNullOrWhiteSpace(Config.Data.MusicPath) && Directory.Exists(Config.Data.MusicPath))
            {
                if (TryReadSongLibraryMigrationState(Config.Data.MusicPath, out string fileOwner, out _) && fileOwner != "" && fileOwner != authedUserId)
                {
                    libraryHasOtherOwner = true;
                    if (!AdoptSongLibraryOnMismatch)
                    {
                        songLibraryOwnerWarning =
                            $"The song library \"{Config.Data.MusicPath}\" is registered for the account \"{fileOwner}\", but you are logged in as \"{authedUserId}\".\n\n" +
                            "Nothing was synced.\n" +
                            "You can log in with the account that owns this library, point this client at another song library, " +
                            "or take the library over for your account (its migration history will be dropped).";
                        Console.WriteLine(songLibraryOwnerWarning);
                        State = "Pull aborted: the song library belongs to another account, nothing was synced.";
                        return;
                    }
                }
            }

            Console.WriteLine($"Pulled {pulledData.Songs.Count()} songs and {pulledData.HistoryEntries.Count()} history entries, writing into local db...");

            LastPulledMigrations = pulledData.Migrations ?? [];
            LastPulledUserId = authedUserId != "" ? authedUserId : pulledData.User?.UserId;

            using var songDbContext = new SongDbContext();
            songDbContext.SongHistoryEntries.RemoveRange(songDbContext.SongHistoryEntries);
            songDbContext.SaveChanges();
            songDbContext.UpvotedSongs.RemoveRange(songDbContext.UpvotedSongs);
            songDbContext.SaveChanges();

            // Add missing user (should just be one, ourselves)
            if (!songDbContext.Users.Where(x => x.UserId == pulledData.User.UserId).Any())
                songDbContext.Users.Add(pulledData.User);
            songDbContext.UpvotedSongs.AddRange(pulledData.Songs);
            songDbContext.SaveChanges();
            songDbContext.SongHistoryEntries.AddRange(pulledData.HistoryEntries);
            songDbContext.SaveChanges();

            // If the user explicitly agreed to take the library over, register it for the current account
            // now (treated as fully migrated for it). Migrations are then applied as usual below, which is
            // a no-op, since the library state was just set to the latest known migration.
            if (libraryHasOtherOwner && AdoptSongLibraryOnMismatch && !string.IsNullOrWhiteSpace(Config.Data.MusicPath))
            {
                int latestKnownNumber = LastPulledMigrations.Length > 0 ? LastPulledMigrations.Max(m => m.MigrationNumber) : 0;
                WriteSongLibraryMigrationState(Config.Data.MusicPath, authedUserId, latestKnownNumber, recordMismatchWarning: false);
                Console.WriteLine($"Song library \"{Config.Data.MusicPath}\" was taken over for account {authedUserId} (treated as fully migrated).");
            }

            // Song library migrations are synced with the pull: apply pending ones (e.g. file renames) to
            // the configured song library. The library has to be known for that, otherwise this is done later
            // once the library folder is set (see Assets.cs).
            if (Config.Data.MusicPath != null)
                ApplySongLibraryMigrations(Config.Data.MusicPath);
        }
        catch (Exception ex)
        {
            State = $"Pull failed: {ex.Message}";
        }
    }

    static void SaveUnsyncedData(string newEntryjson, string endpoint, string? error = null, Guid? SongId = null)
    {
        using var songDbContext = new SongDbContext();
        songDbContext.NotYetSyncedData.Add(new NotYetSyncedData(Guid.NewGuid(), endpoint, newEntryjson, error, SongId));
        songDbContext.SaveChanges();
    }

    public static void UploadNewSong(UpvotedSong newSong)
    {
        var newSongjson = JsonConvert.SerializeObject(newSong, Formatting.Indented);
        try
        {
            var newSongContent = new StringContent(newSongjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/new-song", newSongContent).Result;

            if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.Conflict)
                SaveUnsyncedData(newSongjson, "/sync/new-song", $"{res.IsSuccessStatusCode} {res.Content.ReadAsStringAsync().Result}", newSong.SongId);

            State = $"UploadNewSong {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"UploadNewSong failed: {ex.Message}";

            SaveUnsyncedData(newSongjson, "/sync/new-song", ex.Message, newSong.SongId);
        }
    }

    public static void Vote(SongHistoryEntry newEntry)
    {
        var newEntryjson = JsonConvert.SerializeObject(newEntry, Formatting.Indented);
        try
        {
            var newEntryContent = new StringContent(newEntryjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/vote", newEntryContent).Result;

            if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.Conflict)
                SaveUnsyncedData(newEntryjson, "/sync/vote", $"{res.IsSuccessStatusCode} {res.Content.ReadAsStringAsync().Result}", newEntry.SongId);

            State = $"Vote {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"Vote failed: {ex.Message}";

            SaveUnsyncedData(newEntryjson, "/sync/vote", ex.Message, newEntry.SongId);
        }
    }

    internal static void UpdateVolume(UpvotedSong upvotedSong, float sn)
    {
        var updateVolumeReq = new UpdateVolumeRequest(upvotedSong.SongId, sn);
        var reqJson = JsonConvert.SerializeObject(updateVolumeReq, Formatting.Indented);
        try
        {
            var reqContent = new StringContent(reqJson, Encoding.UTF8, "application/json");
            var res = client.PutAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/volume", reqContent).Result;

            if (!res.IsSuccessStatusCode)
                SaveUnsyncedData(reqJson, "/sync/volume", $"{res.IsSuccessStatusCode} {res.Content.ReadAsStringAsync().Result}", upvotedSong.SongId);

            State = $"UpdateVolume {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"UpdateVolume failed: {ex.Message}";

            SaveUnsyncedData(reqJson, "/sync/volume", ex.Message, upvotedSong.SongId);
        }
    }

    /// <summary>
    /// Tries to create a song library migration on the server. The server assigns the migration number
    /// and renames the matching UpvotedSong rows, so this POST is the commit point of a rename: only
    /// if it succeeds should the client rename the actual file in its song library.
    /// Returns the created migration (including its assigned MigrationNumber) or null if the POST failed.
    /// </summary>
    public static SongLibraryMigration? PostSongLibraryMigration(SongLibraryMigration migration)
    {
        try
        {
            var migrationJson = JsonConvert.SerializeObject(migration, Formatting.Indented);
            var migrationContent = new StringContent(migrationJson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/song-library-migration", migrationContent).Result;

            State = $"PostSongLibraryMigration {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
            if (!res.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<SongLibraryMigration>(res.Content.ReadAsStringAsync().Result);
        }
        catch (Exception ex)
        {
            State = $"PostSongLibraryMigration failed: {ex.Message}";
            return null;
        }
    }

    public static string GetSongLibraryConfigFilePath(string libraryPath) => Path.Combine(libraryPath, SONG_LIBRARY_CONFIG_FILE_NAME);

    /// <summary>
    /// Reads the song library config file. Returns false when the file does not exist or cannot be parsed.
    /// The file has two lines: the user id of the account the song library belongs to, then the number of
    /// the last applied song library migration. Files from before the account check existed only contain
    /// the number (one line); those come back with an empty ownerUserId.
    /// </summary>
    static bool TryReadSongLibraryMigrationState(string libraryPath, out string ownerUserId, out int state)
    {
        ownerUserId = "";
        state = 0;
        try
        {
            string configFilePath = GetSongLibraryConfigFilePath(libraryPath);
            if (!File.Exists(configFilePath))
                return false;

            string[] lines = File.ReadAllText(configFilePath).Replace("\r", "").Split('\n');
            if (lines.Length >= 2 && int.TryParse(lines[1].Trim(), out state))
            {
                ownerUserId = lines[0].Trim(); // New format: account user id + migration number
                return true;
            }
            if (lines.Length >= 1 && int.TryParse(lines[0].Trim(), out state))
                return true; // Legacy format: just the migration number

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TryReadSongLibraryMigrationState failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Writes the migration state of the given song library for the given account: the account user id the
    /// song library belongs to, and the number of the last applied song library migration.
    /// The config file lives in the song library folder, so multiple clients sharing the same library
    /// (e.g. via a NAS) share the file as well, which is why the recorded account matters:
    /// - No (or unparseable) config file: it is created with the given account and number.
    /// - Legacy file (number only, from before the account check existed): adopted for the given account,
    ///   keeping the higher of the two numbers, so no recorded migration is lost.
    /// - File of a different account: unless recordMismatchWarning is false (explicit user consent, see
    ///   <see cref="AdoptSongLibrary"/>), a warning is recorded (see <see cref="TakeSongLibraryOwnerWarning"/>)
    ///   and the file is re-initialized for the current account, treating the library as up to date for it.
    /// - File of the same account: the number is only ever moved forward - if the file already contains a
    ///   higher (or equal) number, another client (e.g. sharing the library via NAS) was faster and the
    ///   file is left untouched.
    /// </summary>
    public static void WriteSongLibraryMigrationState(string libraryPath, string userId, int migrationNumber, bool recordMismatchWarning = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
                return;
            if (string.IsNullOrWhiteSpace(userId))
                return; // Cannot claim ownership of a library without a user id

            string configFilePath = GetSongLibraryConfigFilePath(libraryPath);
            if (!TryReadSongLibraryMigrationState(libraryPath, out string fileOwner, out int fileState))
            {
                File.WriteAllText(configFilePath, $"{userId}\n{migrationNumber}");
                return;
            }

            if (fileOwner == "")
            {
                // Legacy file: no account recorded yet. Adopt it for the current account, keeping the higher number.
                File.WriteAllText(configFilePath, $"{userId}\n{Math.Max(fileState, migrationNumber)}");
                return;
            }

            if (fileOwner != userId)
            {
                int latestKnownNumber = LastPulledMigrations.Length > 0 ? LastPulledMigrations.Max(m => m.MigrationNumber) : 0;
                if (recordMismatchWarning)
                {
                    songLibraryOwnerWarning =
                        $"The song library \"{libraryPath}\" is registered for the account \"{fileOwner}\", but you are logged in as \"{userId}\".\n\n" +
                        "The library will be treated as up to date for your account from now on (the migration history of the other account is dropped).\n" +
                        "If that is not what you want, point this client at the correct song library or log in with the account that owns it.";
                    Console.WriteLine(songLibraryOwnerWarning);
                }
                File.WriteAllText(configFilePath, $"{userId}\n{Math.Max(latestKnownNumber, migrationNumber)}");
                return;
            }

            if (fileState >= migrationNumber)
                return; // Another client of the same account (e.g. sharing the library via NAS) was faster, never regress
            File.WriteAllText(configFilePath, $"{userId}\n{migrationNumber}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WriteSongLibraryMigrationState failed: {ex}");
        }
    }

    /// <summary>
    /// Registers the given song library for the account of the last successful pull, treating it as fully
    /// migrated for that account (its previous migration history is dropped). Meant to be called after the
    /// user explicitly agreed to take a library over that was registered for a different account.
    /// </summary>
    public static void AdoptSongLibrary(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
            return;
        string? authedUserId = LastPulledUserId;
        if (string.IsNullOrWhiteSpace(authedUserId))
            return;

        int latestKnownNumber = LastPulledMigrations.Length > 0 ? LastPulledMigrations.Max(m => m.MigrationNumber) : 0;
        WriteSongLibraryMigrationState(libraryPath, authedUserId, latestKnownNumber, recordMismatchWarning: false);
        Console.WriteLine($"Song library \"{libraryPath}\" was taken over for account {authedUserId} (treated as fully migrated).");
    }

    /// <summary>
    /// Applies the migrations that came with the last pull to the given song library.
    /// The library contains a ".song-library.music-player-config" file that records the account the library
    /// belongs to and the number of the last migration that was applied to it. Only migrations with a
    /// higher number are applied, in order. Migrations are only applied when the recorded account matches
    /// the account of the pull; a library of another account is adopted with a warning instead (treated as
    /// fully migrated), and a missing config file means the library is assumed to be fully migrated
    /// already (only future migrations will be applied).
    /// </summary>
    public static void ApplySongLibraryMigrations(string libraryPath)
    {
        if (LastPulledMigrations.Length == 0)
            return;
        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
            return;

        try
        {
            // The account that made the pull owns the library for the purposes of this check.
            string authedUserId = LastPulledUserId ?? "";
            if (authedUserId == "")
                return;

            bool hasStateFile = TryReadSongLibraryMigrationState(libraryPath, out string fileOwner, out int state);

            if (!hasStateFile)
            {
                // Library has no migration state yet: assume it is fully up to date for the current account.
                int latestNumber = LastPulledMigrations.Max(m => m.MigrationNumber);
                WriteSongLibraryMigrationState(libraryPath, authedUserId, latestNumber);
                Console.WriteLine($"Song library has no migration state file yet, assuming it is up to date for account {authedUserId} (state {latestNumber}).");
                return;
            }

            if (fileOwner != "" && fileOwner != authedUserId)
            {
                // This library carries the migration state of a different account: the numbers are not
                // comparable, so nothing is applied and the state file is left untouched. The pull that
                // brought the migrations normally already aborts in this case (see Pull()); if we end up
                // here anyway (e.g. the library folder was set after the pull), the UI is expected to ask
                // the user whether they want to take the library over (see AdoptSongLibrary).
                songLibraryOwnerWarning =
                    $"The song library \"{libraryPath}\" is registered for the account \"{fileOwner}\", but you are logged in as \"{authedUserId}\".\n\n" +
                    "Nothing was applied to it.\n" +
                    "You can log in with the account that owns this library, point this client at another song library, " +
                    "or take the library over for your account (its migration history will be dropped).";
                Console.WriteLine(songLibraryOwnerWarning);
                return;
            }

            if (fileOwner == "")
            {
                // Legacy state file (number only, from before the account check existed): adopt it for the
                // current account without losing the recorded number, so only future migrations get applied.
                WriteSongLibraryMigrationState(libraryPath, authedUserId, state);
            }

            var pendingMigrations = LastPulledMigrations
                .Where(m => m.MigrationNumber > state)
                .OrderBy(m => m.MigrationNumber)
                .ToArray();
            if (pendingMigrations.Length == 0)
                return;

            int highestApplied = state;
            foreach (var migration in pendingMigrations)
            {
                if (migration.MigrationType == SongLibraryMigrationType.Rename)
                {
                    if (string.IsNullOrWhiteSpace(migration.OldName) || string.IsNullOrWhiteSpace(migration.NewName))
                    {
                        highestApplied = migration.MigrationNumber; // Nothing sensible to do, dont get stuck on it
                        continue;
                    }

                    // The migration refers to one specific song entry and snapshots its album/artist (a file
                    // rename does not change the tags of the file). Only rename the files that really belong
                    // to this song; other files with the same name but different tags are different songs.
                    // Entries without album/artist metadata can only be identified by their file name.
                    var filesToRename = FindSongFilesByName(libraryPath, migration.OldName)
                        .Where(f => SongFileMatchesTags(f, migration.Artist, migration.Album))
                        .ToList();

                    bool allRenamesSucceeded = true;
                    foreach (string oldFilePath in filesToRename)
                    {
                        string newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath) ?? libraryPath, migration.NewName);
                        if (File.Exists(newFilePath))
                            continue; // Target already exists, nothing to do

                        try
                        {
                            File.Move(oldFilePath, newFilePath);
                            Console.WriteLine($"Applied song library migration #{migration.MigrationNumber}: renamed \"{oldFilePath}\" to \"{newFilePath}\".");
                        }
                        catch (Exception ex)
                        {
                            if (!File.Exists(oldFilePath))
                                continue; // The file vanished in the meantime (e.g. another client already renamed it), nothing to do

                            // Keep the old state so this migration is retried on the next startup.
                            Console.WriteLine($"Could not apply song library migration #{migration.MigrationNumber} ({migration.OldName} -> {migration.NewName}): {ex}");
                            allRenamesSucceeded = false;
                        }
                    }
                    if (!allRenamesSucceeded)
                        break;
                }
                else if (migration.MigrationType == SongLibraryMigrationType.Delete)
                {
                    // The deleted entry is already gone from the database (the pull rewrote it), but the
                    // migration snapshots its album/artist. Only delete the files that really belong to this
                    // song, so the files of same-named other songs (different tags) survive. Entries without
                    // album/artist metadata can only be identified by their file name.
                    var filesToDelete = FindSongFilesByName(libraryPath, migration.OldName)
                        .Where(f => SongFileMatchesTags(f, migration.Artist, migration.Album))
                        .ToList();

                    // Delete the files from the library. Clients that share the library via a NAS usually find
                    // nothing to do here, since the deleting client already removed the file.
                    foreach (string oldFilePath in filesToDelete)
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                            Console.WriteLine($"Applied song library migration #{migration.MigrationNumber}: deleted \"{oldFilePath}\".");
                        }
                        catch (Exception ex)
                        {
                            if (!File.Exists(oldFilePath))
                                continue; // The file vanished in the meantime (e.g. another client already deleted it), nothing to do

                            Console.WriteLine($"Could not apply song library migration #{migration.MigrationNumber} (delete {migration.OldName}): {ex}");
                            return;
                        }
                    }
                }

                highestApplied = migration.MigrationNumber;
            }

            // Never regress a state another client (sharing the library via NAS) already wrote in the meantime.
            if (highestApplied != state)
                WriteSongLibraryMigrationState(libraryPath, authedUserId, highestApplied);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ApplySongLibraryMigrations failed: {ex}");
        }
    }

    /// <summary>
    /// Checks whether the tags of the given song file match the album/artist of a song (same convention
    /// the database rows are filled with, see MusicPlayerSyncInterface.SongFileMatching). An empty album
    /// and artist mean the song carries no metadata and can only be identified by its file name, in which
    /// case any file with that name counts as a match. Files whose tags cannot be read only match songs
    /// without metadata.
    /// </summary>
    public static bool SongFileMatchesTags(string filePath, string artist, string album)
    {
        if (SongFileMatching.HasNoAlbumOrArtist(artist, album))
            return true; // No metadata to compare against: the file name is all the identity there is

        try
        {
            using (TagLib.File file = TagLib.File.Create(filePath))
            {
                string fileAlbum = file.Tag.Album ?? "";
                string fileArtists = file.Tag.AlbumArtists.Length == 0 ? "" : file.Tag.AlbumArtists.Aggregate((x, y) => x + " + " + y);
                return SongFileMatching.TagsEqual(artist, album, fileArtists, fileAlbum);
            }
        }
        catch
        {
            return false; // Could not read the tags: do not touch a file that cannot be identified
        }
    }

    /// <summary>
    /// Checks whether the tags of the given song file match the given upvotedSong entry (see
    /// <see cref="SongFileMatchesTags"/>).
    /// </summary>
    public static bool SongFileMatchesEntry(string filePath, UpvotedSong entry) => SongFileMatchesTags(filePath, entry.Artist, entry.Album);

    /// <summary>
    /// Recursively finds all files with the given file name in the song library.
    /// </summary>
    public static List<string> FindSongFilesByName(string startDir, string fileName)
    {
        List<string> foundFiles = [];
        foreach (string filePath in Directory.GetFiles(startDir))
            if (string.Equals(Path.GetFileName(filePath), fileName, StringComparison.OrdinalIgnoreCase))
                foundFiles.Add(filePath);

        foreach (string subDir in Directory.GetDirectories(startDir))
            foundFiles.AddRange(FindSongFilesByName(subDir, fileName));

        return foundFiles;
    }
}