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
        client?.Dispose();

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

            using var songDbContext = new SongDbContext();
            songDbContext.SongHistoryEntries.RemoveRange(songDbContext.SongHistoryEntries);
            songDbContext.SaveChanges();
            songDbContext.UpvotedSongs.RemoveRange(songDbContext.UpvotedSongs);
            songDbContext.SaveChanges();

            if (!songDbContext.Users.Where(x => x.UserId == pulledData.users.FirstOrDefault().UserId).Any())
                songDbContext.Users.Add(pulledData.users.FirstOrDefault());
            songDbContext.UpvotedSongs.AddRange(pulledData.songs);
            songDbContext.SaveChanges();
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
        try
        {
            var newSongjson = JsonConvert.SerializeObject(newSong, Formatting.Indented);
            var newSongContent = new StringContent(newSongjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}/sync/new-song", newSongContent).Result;

            State = $"UploadNewSong {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"UploadNewSong failed: {ex.Message}";
        }
    }

    public static void Vote(SongHistoryEntry newEntry)
    {
        try
        {
            var newEntryjson = JsonConvert.SerializeObject(newEntry, Formatting.Indented);
            var newEntryContent = new StringContent(newEntryjson, Encoding.UTF8, "application/json");
            var res = client.PostAsync($"{Config.Data.SyncServerHost}/sync/vote", newEntryContent).Result;

            State = $"Vote {res.StatusCode} {res.Content.ReadAsStringAsync().Result}";
        }
        catch (Exception ex)
        {
            State = $"Vote failed: {ex.Message}";
        }
    }
}