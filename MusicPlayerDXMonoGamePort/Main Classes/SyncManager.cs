using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EzAuth.Interfaces;
using EzAuth.Keycloak;
using MusicPlayerDXMonoGamePort.Persistence.Database;
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

    public static void Pull()
    {
        try
        {
            var res = client.GetStringAsync($"{Config.Data.SyncServerHost}{ROUTE_VERSION_PREFIX}/sync/pull").Result;
            var pulledData = JsonConvert.DeserializeObject<SyncPullResponse>(res);

            if (pulledData == null)
                throw new Exception("Pulled data was null!");
            if (pulledData.Songs.Count() == 0 || pulledData.HistoryEntries.Count() == 0)
                throw new Exception("Pulled data was empty!");

            Console.WriteLine($"Pulled {pulledData.Songs.Count()} songs and {pulledData.HistoryEntries.Count()} history entries, writing into local db...");

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
}