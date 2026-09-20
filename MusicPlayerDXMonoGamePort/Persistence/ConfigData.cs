using MusicPlayerSyncInterface.DTOs;
using System.Collections.Generic;
using System.Drawing;

namespace Persistence
{
    public class ConfigData
    {
        // Visualization setting fake enums
        public int Background = 1;
        public int Vis = 2;

        public bool AntiAliasing = true;
        public bool AutoStopDiscordRpcOnGameDetection = true;
        public bool AutoVolume = false;
        public Color BackgroundColor = Color.White;
        public string BrowserDownloadFolderPath;
        public Color Col = Color.Transparent;
        public bool DiscordRpcActive = true;
        public bool firstStart = true;
        public bool MultiThreading = true;
        public string MusicPath;
        public bool OldSmooth = true;
        public bool Preload = true;
        public int ShadowDistance = 5;
        public float Smoothness = 0.5f;
        public float Volume = 0.215f;
        public int WavePreload = 1;
        public Point WindowPos = new Point(0, 0);

        // Sync settings
        public string? AuthBackendRefreshToken;
        public string? SyncServerHost;
        public string? SyncServerUsername;

        /// <summary>
        /// The incremental history pull cursor per account (UserId): the highest history sequence this
        /// client already holds. It is sent as "historySince" on the next pull, so the server only sends
        /// the entries that are new instead of the whole (forever growing) history. Kept out of the song
        /// library state file on purpose - that file is shared between devices through the NAS, while the
        /// cursor describes THIS local database. A missing entry (0) means "no cursor yet": the pull then
        /// bootstraps (verifies the newest entries and adopts the cursor) or falls back to the full history.
        /// </summary>
        public Dictionary<string, long> SyncHistorySequences = new();

        public List<UpvotedSong> songDatabaseEntries;

        public ConfigData()
        {
            songDatabaseEntries = [];
        }
    }
}
