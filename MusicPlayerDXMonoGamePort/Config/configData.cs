using MusicPlayerDXMonoGamePort;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Configuration
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

        public List<UpvotedSong> songDatabaseEntries;

        public ConfigData()
        {
            songDatabaseEntries = new List<UpvotedSong>();
        }
    }

    public class UpvotedSong
    {
        public UpvotedSong(string Name, float Score, int Streak, int TotalLikes, int TotalDislikes, long AddingDates, float Volume)
        {
            this.Name = Name;
            this.Score = Score;
            this.Streak = Streak;
            this.TotalLikes = TotalLikes;
            this.TotalDislikes = TotalDislikes;
            this.AddingDates = AddingDates;
            this.Volume = Volume;
        }

        public string Name;
        public float Score;
        public int Streak;
        public int TotalLikes;
        public int TotalDislikes;
        public long AddingDates;
        public float Volume;

        public string Path; // Only used in ExportChooser.cs
    }
}
