using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using EzAuth.Keycloak;
using MusicPlayerDXMonoGamePort.Persistence.Database;
using MusicPlayerSyncInterface.DTOs;
using Newtonsoft.Json;
using Persistence;

namespace MusicPlayerDXMonoGamePort.Main_Classes;

public static class SyncManager
{
    static HttpClient? _httpClient = new();
    static KeyCloakHttpClient client = null;
    static KeyCloakAddress keyCloakAddress = null;
    public static string State { get => state; private set { OnStateChanged?.Invoke(value); state = value; } }
    private static string state = "";
    public static Action<string>? OnStateChanged = null;

    static SyncManager()
    {
        Init();
    }

    public static void Init(string? password = null, bool TryCallApiInit = false)
    {
        try
        {
            keyCloakAddress = GetKeycloakAddress(Config.Data.SyncServerHost);
            client = new(keyCloakAddress, (string newKeycloakRefreshToken) =>
                {
                    Config.Data.KeycloakRefreshToken = newKeycloakRefreshToken;
                    Config.Save();
                }, Config.Data.KeycloakRefreshToken, _httpClient);

            if (password != null)
                client.Login(Config.Data.SyncServerUsername, password);
        }
        catch (Exception ex)
        {
            State = $"SyncManager Init failed: {ex.Message}";
            return;
        }

        try
        {
            if (TryCallApiInit)
            {
                using var songDbContext = new SongDbContext();
                var sendObjString = JsonConvert.SerializeObject(new UserSongDataAndHistory([], songDbContext.UpvotedSongs.ToArray(), songDbContext.SongHistoryEntries.ToArray()), Formatting.Indented);
                var sendContent = new StringContent(sendObjString, Encoding.UTF8, "application/json");
                var res = client.PostAsync($"{Config.Data.SyncServerHost}/sync/init", sendContent).Result;
                State = $"Init {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
            }
        }
        catch (Exception ex)
        {
            State = $"API Init failed: {ex.Message}";
        }
    }

    public static KeyCloakAddress GetKeycloakAddress(string syncServerHost)
    {
        var res = _httpClient.GetAsync($"{syncServerHost}/keycloak").Result;
        var content = res.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<KeyCloakAddress>(content);
    }

    public static void Pull()
    {
        try
        {
            var res = client.GetStringAsync($"{Config.Data.SyncServerHost}/sync/pull").Result;
            var pulledData = JsonConvert.DeserializeObject<UserSongDataAndHistory>(res);

            if (pulledData == null)
                throw new Exception("Pulled data was null!");
            if (pulledData.songs.Count() == 0 || pulledData.historyEntries.Count() == 0)
                throw new Exception("Pulled data was empty!");

            using var songDbContext = new SongDbContext();
            songDbContext.SongHistoryEntries.RemoveRange(songDbContext.SongHistoryEntries);
            songDbContext.UpvotedSongs.RemoveRange(songDbContext.UpvotedSongs);

            if (!songDbContext.Users.Where(x => x.UserId == pulledData.users.FirstOrDefault().UserId).Any())
                songDbContext.Users.Add(pulledData.users.FirstOrDefault());
            songDbContext.UpvotedSongs.AddRange(pulledData.songs);
            songDbContext.SongHistoryEntries.AddRange(pulledData.historyEntries);
            songDbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            State = $"Pull failed: {ex.Message}";
        }
    }

    public static void UploadNewSong(UpvotedSong newSong)
    {
        void SaveUnsyncedData(string newSongjson, string? error = null)
        {
            using var songDbContext = new SongDbContext();
            songDbContext.NotYetSyncedData.Add(new NotYetSyncedData(Guid.NewGuid(), "/sync/new-song", newSongjson, error));
            songDbContext.SaveChanges();
        }

        var newSongjson = JsonConvert.SerializeObject(newSong, Formatting.Indented);
        try
        {
            var newSongContent = new StringContent(newSongjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}/sync/new-song", newSongContent).Result;

            if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.Conflict)
                SaveUnsyncedData(newSongjson, $"{res.IsSuccessStatusCode} {res.Content.ReadAsStringAsync().Result}");

            State = $"UploadNewSong {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"UploadNewSong failed: {ex.Message}";

            SaveUnsyncedData(newSongjson, ex.Message);
        }
    }

    public static void Vote(SongHistoryEntry newEntry)
    {
        void SaveUnsyncedData(string newEntryjson, string? error = null)
        {
            using var songDbContext = new SongDbContext();
            songDbContext.NotYetSyncedData.Add(new NotYetSyncedData(Guid.NewGuid(), "/sync/vote", newEntryjson, error, newEntry.SongId));
            songDbContext.SaveChanges();
        }

        var newEntryjson = JsonConvert.SerializeObject(newEntry, Formatting.Indented);
        try
        {
            var newEntryContent = new StringContent(newEntryjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}/sync/vote", newEntryContent).Result;

            if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.Conflict)
                SaveUnsyncedData(newEntryjson, $"{res.IsSuccessStatusCode} {res.Content.ReadAsStringAsync().Result}");

            State = $"Vote {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"Vote failed: {ex.Message}";

            SaveUnsyncedData(newEntryjson, ex.Message);
        }
    }
}