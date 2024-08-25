using Persistence;
using Microsoft.Xna.Framework.Media;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Persistence.Database;
using MusicPlayerDXMonoGamePort.Persistence.Database;

namespace MusicPlayerDXMonoGamePort
{
    public class SongManager
    {
        // Music Player Manager Values
        public static string currentlyPlayingSongName
        {
            get => PlayerHistory[PlayerHistoryIndex].Split('\\').Last();
        }
        public static string currentlyPlayingSongPath
        {
            get => PlayerHistory[PlayerHistoryIndex];
        }
        public static string previouslyPlayingSongName
        {
            get => PlayerHistory[PlayerHistoryIndex - 1].Split('\\').Last();
        }
        public static string previouslyPlayingSongPath
        {
            get => PlayerHistory[PlayerHistoryIndex - 1];
        }
        public static List<string> Playlist = new List<string>();
        public static List<string> PlayerHistory = new List<string>();
        public static int PlayerHistoryIndex = -1;
        public static int SongChangedTickTime = -100000;
        public static int SongStartTime;
        public static bool IsCurrentSongUpvoted;
        public static int LastUpvotedSongStreak;
        static List<string> SongChoosingList = new List<string>();
        public static float LastScoreChange = 0;

        // NAudio
        public static WaveChannel32 Channel32;
        public static WaveChannel32 Channel32Reader;
        public static WaveChannel32 Channel32ReaderThreaded;
        public static DirectSoundOut output;
        public static Mp3FileReader mp3;
        public static Mp3FileReader mp3Reader;
        public static Mp3FileReader mp3ReaderThreaded;
        public static MMDevice device;
        public static MMDeviceEnumerator enumerator;

        // History Data
        public static readonly string historyFilePath = Values.CurrentExecutablePath + "\\History.txt";
        public static List<HistorySong> HistorySongData = new List<HistorySong>();

        // Song Data
        public static UpvotedSong currentlyPlayingSongData;
        static int HighestSongRatioInR = 0;
        public struct DistancePerSong
        {
            public int SongIndex;
            public float SongDifference;
        }

        // MultiThreading
        public static Task T = null;
        public static bool AbortAbort = false;
        public static bool SavingToFileRightNow = false;

        // Debug
        public static long CurrentDebugTime = 0;
        public static long CurrentDebugTime2 = 0;

        // Player Managment
        public static void PlayPause()
        {
            Task PlayPause = Task.Factory.StartNew(() =>
            {
                if (output != null)
                {
                    if (output.PlaybackState == PlaybackState.Playing)
                    {
                        output.Pause(); // NAudio can get stuck here for some reason, which stopped the main thread in older versions
                    }
                    else if (output.PlaybackState == PlaybackState.Paused || output.PlaybackState == PlaybackState.Stopped)
                    {
                        output.Play();
                    }
                    Program.game.UpdateDiscordRPC();
                }
            });
        }
        public static bool IsPlaying()
        {
            if (output == null) return false;
            else if (output.PlaybackState == PlaybackState.Playing) return true;
            return false;
        }

        // Start Song
        public static void PlayFirstSong()
        {
            foreach (UpvotedSong s in DbHolder.DbContext.UpvotedSongs)
                s.Path = GetSongPathFromSongName(s.Name);
            HighestSongRatioInR = DbHolder.DbContext.UpvotedSongs.ToList().Max(x => float.IsInfinity(x.TotalLikes / (float)x.TotalDislikes) ? int.MinValue : (int)(x.TotalLikes / (float)x.TotalDislikes));
            CreateSongChoosingList();
            if (Playlist.Count > 0)
            {
                if (Program.args.Length > 0)
                    PlayNewSong(Program.args[0]);
                else if (false && File.Exists(historyFilePath))
                {
                    try
                    {
                        string songName = File.ReadLines(historyFilePath).Last();

                        if (!songName.EndsWith(".mp3"))
                            songName += ".mp3";

                        PlayPlaylistSong(songName);
                    }
                    catch
                    {
                        int PlaylistIndex = Values.RDM.Next(Playlist.Count);
                        GetNextSong(true, false);
                        PlayerHistory.Add(Playlist[PlaylistIndex]);
                    }
                }
                else
                {
                    int PlaylistIndex = Values.RDM.Next(Playlist.Count);
                    GetNextSong(true, false);
                    PlayerHistory.Add(Playlist[PlaylistIndex]);
                }
            }
            else
                Console.WriteLine("Playlist empty!");
        }
        public static bool PlayNewSong(string sPath)
        {
            if (Values.Timer > SongChangedTickTime + 10 && !Config.Data.MultiThreading ||
                Config.Data.MultiThreading)
            {
                SaveUserSettings(true);

                sPath = sPath.Trim('"');

                if (!File.Exists(sPath))
                {
                    List<string> Choosing = Playlist.OrderBy(x => Values.RDM.NextDouble()).ToList();
                    DistancePerSong[] LDistances = new DistancePerSong[Choosing.Count];
                    for (int i = 0; i < LDistances.Length; i++)
                    {
                        LDistances[i].SongDifference = Values.LevenshteinDistanceWrapper(sPath, Path.GetFileNameWithoutExtension(Choosing[i]));
                        LDistances[i].SongIndex = i;
                    }

                    LDistances = LDistances.OrderBy(x => x.SongDifference).ToArray();
                    int NonWorkingIndexes = 0;
                    sPath = Choosing[LDistances[NonWorkingIndexes].SongIndex];
                    while (!File.Exists(sPath))
                    {
                        NonWorkingIndexes++;
                        sPath = Choosing[LDistances[NonWorkingIndexes].SongIndex];
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">Found one matching song: \"" + Path.GetFileNameWithoutExtension(sPath) + "\" with a difference of " +
                        Math.Round(LDistances[NonWorkingIndexes].SongDifference, 2));

                    for (int i = 1; i <= 5; i++)
                    {
                        if (LDistances[NonWorkingIndexes + i].SongDifference > 2)
                            break;
                        if (i == 1)
                            Console.WriteLine("Other well fitting songs were:");
                        Console.WriteLine(i + ". \"" + Path.GetFileNameWithoutExtension(Choosing[LDistances[NonWorkingIndexes + i].SongIndex]) + "\" with a difference of " +
                            Math.Round(LDistances[NonWorkingIndexes + i].SongDifference, 2));
                    }
                }

                PlayerHistory.Add(sPath);
                PlayerHistoryIndex = PlayerHistory.Count - 1;

                if (!Playlist.Contains(sPath))
                    Playlist.Add(sPath);

                try
                {
                    PlaySongByPath(sPath);
                }
                catch
                {
                    MessageBox.Show("That song is not readable!");
                    PlayerHistory.Remove(sPath);
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    GetNextSong(true, false);
                }

                SongChangedTickTime = (int)Values.Timer;
                return true;
            }
            return false;
        }
        public static bool PlayPlaylistSong(string SongNameWithFileEnd)
        {
            for (int i = 0; i < Playlist.Count; i++)
            {
                if (Playlist[i].Split('\\').Last() == SongNameWithFileEnd)
                {
                    SaveUserSettings(true);

                    PlayerHistory.Add(Playlist[i]);
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    PlaySongByPath(Playlist[i]);
                    return true;
                }
            }
            return false;
        }
        private static void PlaySongByPath(string PathString)
        {
            if (!File.Exists(PathString))
            {
                Playlist.Remove(PathString);
                PlayerHistory.RemoveAt(PlayerHistoryIndex);
                PlayerHistoryIndex--;
                if (PlayerHistoryIndex < 0)
                    PlayerHistoryIndex = 0;
                GetNextSong(true, false);
                return;
            }

            var upvotedSongsList = DbHolder.DbContext.UpvotedSongs.ToList();
            int index = upvotedSongsList.FindIndex(x => x.Name == currentlyPlayingSongName);
            if (index != -1 && upvotedSongsList[index].Volume != -1)
            {
                float mult = Values.BaseVolume / upvotedSongsList[index].Volume;
                Values.VolumeMultiplier = mult;
                //Program.game.ShowSecondRowMessage("Applied Volume multiplier of: " + Math.Round(mult, 2), 1);
            }
            else if (Values.VolumeMultiplier > 2 || Values.VolumeMultiplier < 0.1f)
                Values.VolumeMultiplier = 1;

            Config.Data.Preload = Program.game.Preload;
            KeyHookManager.CreateGlobalKeyHooks(Program.game.gameWindowForm.Handle);
            if (T != null && T.Status == TaskStatus.Running)
            {
                AbortAbort = true;
                T.Wait();
            }

            XnaGuiManager.SongTimeSkipped = 0;
            Program.game.ForcedCoverBackgroundRedraw = true;
            Program.game.ForceTitleRedraw(true);
            if (Program.game.DG != null)
                Program.game.DG.Clear();

            DisposeNAudioData();

            if (PathString.Contains("\""))
                PathString = PathString.Trim(new char[] { '"', ' ' });

            mp3 = new Mp3FileReader(PathString);
            mp3Reader = new Mp3FileReader(PathString);
            mp3ReaderThreaded = new Mp3FileReader(PathString);
            Channel32 = new WaveChannel32(mp3);
            Channel32Reader = new WaveChannel32(mp3Reader);
            Channel32ReaderThreaded = new WaveChannel32(mp3ReaderThreaded);

            output = new DirectSoundOut();
            output.Init(Channel32);

            if (Config.Data.Preload || DbHolder.DbContext.UpvotedSongs.ElementAt(index).Volume == -1)
            {
                if (Config.Data.MultiThreading)
                    T = Task.Factory.StartNew(SongVisualization.UpdateEntireSongBuffers);
                else
                    SongVisualization.UpdateEntireSongBuffers();
            }

            output.Play();
            Channel32.Volume = 0;
            SongStartTime = (int)Values.Timer;
            Channel32.Position = SongVisualization.bufferLength / 2;

            currentlyPlayingSongData = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == currentlyPlayingSongName);
            AddSongToListIfNotDoneSoFar(currentlyPlayingSongPath);
            //Program.game.UpdateDiscordRPC();
        }

        // Song Choosing
        private static void GetNewPlaylistSong()
        {
            // TODO: Real time song choosing
            CurrentDebugTime = Stopwatch.GetTimestamp();

            double random = Values.RDM.NextDouble();
            if (random < 0.3)
            {
                // Just get a song from the Choosing List
                PlaySongFromTheChoosingList();
            }
            else if (random < 0.85)
            {
                // Play a song that hasnt been played yet
                var upvotedSongsList = DbHolder.DbContext.UpvotedSongs.ToList();
                int index = upvotedSongsList.FindIndex(x => x.Streak == 0);
                if (index != -1)
                {
                    // If there is one play it
                    PlayerHistory.Add(GetSongPathFromSongName(upvotedSongsList[index].Name));
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
                }
                else
                {
                    // No unplayed song found get one from the choosing list
                    PlaySongFromTheChoosingList();
                }
            }
            else
            {
                // Play a song that has been upvoted recently
                List<string> RecentlyPlayedChoosingList = new List<string>();
                for (int i = 4; i < 10; i++)
                {
                    if (HistorySongData.Count > i && HistorySongData[HistorySongData.Count - 1 - i].Change > 0)
                    {
                        UpvotedSong S = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(X => X.Name == HistorySongData[i].Name + ".mp3");
                        if (S != null && S.Score < 120)
                        {
                            for (int j = 0; j < HistorySongData[i].Change * 50; j++)
                                RecentlyPlayedChoosingList.Add(HistorySongData[i].Name);
                        }
                    }
                }
                if (RecentlyPlayedChoosingList.Count > 0)
                {
                    // Play a recently upvoted song
                    PlayerHistory.Add(GetSongPathFromSongName(RecentlyPlayedChoosingList[Values.RDM.Next(RecentlyPlayedChoosingList.Count)]));
                    PlayerHistoryIndex = PlayerHistory.Count - 1;
                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
                }
                else
                {
                    // No recently upvoted song found
                    PlaySongFromTheChoosingList();
                }
            }

            Debug.WriteLine("New Song calc time: " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
        }
        private static void PlaySongFromTheChoosingList()
        {
            int SongChoosingListIndex = 0;

            do
                SongChoosingListIndex = Values.RDM.Next(SongChoosingList.Count);
            while (PlayerHistory.Count != 0 && SongChoosingList[SongChoosingListIndex] == PlayerHistory[PlayerHistoryIndex - 1] && Playlist.Count > 1);

            PlayerHistory.Add(SongChoosingList[SongChoosingListIndex]);
            PlayerHistoryIndex = PlayerHistory.Count - 1;
            PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
        }
        public static void CreateSongChoosingList()
        {
            //CurrentDebugTime = Stopwatch.GetTimestamp();

            SongChoosingList.Clear();
            for (int i = 0; i < Playlist.Count; i++)
            {
                SongChoosingList.Add(Playlist[i]);

                string SongName = Playlist[i].Split('\\').Last();
                float amount = GetSongChoosingAmount(DbHolder.DbContext.UpvotedSongs.First(x => x.Name == SongName)) + 1;

                for (int k = 0; k < amount; k++)
                    SongChoosingList.Add(Playlist[i]);
            }
#if DEBUG
            // testing shit
            TestChoosingListIntegrity();
#endif
            //Debug.WriteLine("SongChoosing List update time: " + (Stopwatch.GetTimestamp() - CurrentDebugTime) + " length: " + SongChoosingList.Count);
        }
        public static void UpdateSongChoosingList(string SongPath)
        {
            CurrentDebugTime2 = Stopwatch.GetTimestamp();

            // TODO: Fix doubled chance
            string SongName = SongPath.Split('\\').Last();

            // Getting Choosing List Count
            int index = SongChoosingList.FindIndex(x => x == SongPath);
            if (index == -1)
            {
                //Console.WriteLine("oi that song doesnt even exist lol");
                return;
            }
            int i = index;
            while (i < SongChoosingList.Count && SongChoosingList[i] == SongPath)
                i++;
            int count = i - index;

            // Getting target Count
            int amount = (int)GetSongChoosingAmount(DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == SongName)) + 1;

            for (int j = 0; j < amount - count; j++)
                SongChoosingList.Insert(index, SongPath);
            for (int j = 0; j < count - amount; j++)
                SongChoosingList.RemoveAt(index);

            Debug.WriteLine($"SongChoosingList Time: {Stopwatch.GetTimestamp() - CurrentDebugTime2} - Size {SongChoosingList.Count}");

#if DEBUG
            // testing shit
            TestChoosingListIntegrity();
#endif
        }
        public static float GetSongChoosingAmount(UpvotedSong curSong)
        {
            float amount = 0;
            float ChanceIncreasePerUpvote = 1000f / Playlist.Count;
            if (curSong != null)
            {
                switch (0) // Im keeping old choosing algorithms so I can experiment
                {
                    case 0: // Default choosing
                        // Give songs with good ratio extra chance
                        float ratio = 0;
                        if (curSong.TotalDislikes > 0)
                            ratio = curSong.TotalLikes / (float)curSong.TotalDislikes;
                        else if (curSong.TotalLikes > 0)
                            ratio = float.MaxValue;
                        amount += (Values.Sigmoid(ratio) - 0.5f) * 100 * ChanceIncreasePerUpvote;

                        // Give songs with good score extra chance
                        if (curSong.Score > 0)
                            amount += (int)(Math.Ceiling(curSong.Score * ChanceIncreasePerUpvote));

                        // Give young songs extra chance
                        float age = SongAge(curSong);
                        if (age < 30)
                            amount += (int)((30 - age) * ChanceIncreasePerUpvote * 60f / 30f);
                        break;

                    case 1: // Ratio only choosing
                        amount = (float)curSong.TotalLikes / curSong.TotalDislikes;
                        if (float.IsInfinity(amount))
                            amount = HighestSongRatioInR + 100;
                        break;
                }
            }
            return amount;
        }

        // Move through PlayerHistory
        public static void GetNextSong(bool forced, bool DownVoteCurrentSongForUserSkip)
        {
            if (Config.Data.MultiThreading || forced ||
                Values.Timer > SongChangedTickTime + 5 && !Config.Data.MultiThreading)
            {
                output?.Pause();

                if (PlayerHistory.Count > 0)
                    DownvoteCurrentSongIfNeccesary(DownVoteCurrentSongForUserSkip);

                SaveUserSettings(true);

                PlayerHistoryIndex++;
                if (PlayerHistoryIndex > PlayerHistory.Count - 1)
                    GetNewPlaylistSong();
                else
                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);

                SongChangedTickTime = (int)Values.Timer;
            }
        }
        public static void GetPreviousSong()
        {
            if (Values.Timer > SongChangedTickTime + 5 && !Config.Data.MultiThreading ||
                Config.Data.MultiThreading)
            {
                SaveUserSettings(true);

                if (PlayerHistoryIndex > 0)
                {
                    PlayerHistoryIndex--;

                    PlaySongByPath(PlayerHistory[PlayerHistoryIndex]);
                }

                SongChangedTickTime = (int)Values.Timer;
            }
        }

        // Queue next Songs
        public static void QueueNewSong(string Song, bool ConsoleOutput)
        {
            if (!File.Exists(Song))
            {
                DistancePerSong[] LDistances = new DistancePerSong[Playlist.Count];
                for (int i = 0; i < LDistances.Length; i++)
                {
                    LDistances[i].SongDifference = Values.LevenshteinDistanceWrapper(Song, Path.GetFileNameWithoutExtension(Playlist[i]));
                    LDistances[i].SongIndex = i;
                }

                LDistances = LDistances.OrderBy(x => x.SongDifference).ToArray();
                int NonWorkingIndexes = 0;
                Song = Playlist[LDistances[NonWorkingIndexes].SongIndex];
                while (!File.Exists(Song))
                {
                    NonWorkingIndexes++;
                    Song = Playlist[LDistances[NonWorkingIndexes].SongIndex];
                }

                if (ConsoleOutput)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">Queued one matching song: \"" + Path.GetFileNameWithoutExtension(Song) + "\" with a difference of " +
                        Math.Round(LDistances[NonWorkingIndexes].SongDifference, 2));

                    for (int i = 1; i <= 5; i++)
                    {
                        if (LDistances[NonWorkingIndexes + i].SongDifference > 2)
                            break;
                        if (i == 1)
                            Console.WriteLine("Other well fitting songs were:");
                        Console.WriteLine(i + ". \"" + Path.GetFileNameWithoutExtension(Playlist[LDistances[NonWorkingIndexes + i].SongIndex]) + "\" with a difference of " +
                            Math.Round(LDistances[NonWorkingIndexes + i].SongDifference, 2));
                    }
                }
            }

            QueueSong(Song);
        }
        public static void QueueSong(string Song)
        {
            Program.game.ShowSecondRowMessage("Added  a  song  to  the  queue!", 1f);
            PlayerHistory.Add(Song);
        }

        // Song Metadata Management
        private static float GetUpvoteWeight(float SongScore)
        {
            return (float)Math.Pow(2, -SongScore / 20);
        }
        private static float GetDownvoteWeight(float SongScore)
        {
            return (float)Math.Pow(2, (SongScore - 100) / 20);
        }
        private static void DownvoteCurrentSongIfNeccesary(bool DownVoteCurrentSongForUserSkip)
        {
            if (PlayerHistoryIndex != -1)
            {
                var upvotedSong = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == currentlyPlayingSongName);

                if (upvotedSong != null && DownVoteCurrentSongForUserSkip && PlayerHistoryIndex == PlayerHistory.Count - 1 && !IsCurrentSongUpvoted)
                {
                    float percentage = (Channel32.Position - XnaGuiManager.SongTimeSkipped) / (float)Channel32.Length;

                    if (upvotedSong.Score > 120)
                        upvotedSong.Score = 120;
                    if (upvotedSong.Score < -1)
                        upvotedSong.Score = -1;

                    if (upvotedSong.Streak > -1)
                        upvotedSong.Streak = -1;
                    else
                        upvotedSong.Streak -= 1;

                    LastScoreChange = upvotedSong.Streak * GetDownvoteWeight(upvotedSong.Score) * 32 * (1 - percentage);
                    upvotedSong.Score += upvotedSong.Streak * GetDownvoteWeight(upvotedSong.Score) * 32 * (1 - percentage);

                    Program.game.ShowSecondRowMessage("Downvoted  previous  song!", 1.2f);
                    upvotedSong.TotalDislikes++;
                    SaveUserSettings(false);

                    UpdateSongChoosingList(currentlyPlayingSongPath);
                }
            }
        }
        private static void UpvoteCurrentSongIfNeccesary()
        {
            if (IsCurrentSongUpvoted)
            {
                Program.game.UpvoteSavedAlpha = 1.4f;

                AddSongToListIfNotDoneSoFar(currentlyPlayingSongPath);

                var upvotedSong = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == currentlyPlayingSongName);
                double percentage;
                if (Channel32 == null)
                    percentage = 1;
                else
                    percentage = (Channel32.Position - XnaGuiManager.SongTimeSkipped) / (double)Channel32.Length;

                if (upvotedSong.Score > 120)
                    upvotedSong.Score = 120;
                if (upvotedSong.Score < -1)
                    upvotedSong.Score = -1;

                if (upvotedSong.Streak < 1)
                    upvotedSong.Streak = 1;
                else if (Channel32 != null && Channel32.Position > Channel32.Length - SongVisualization.bufferLength / 2)
                    upvotedSong.Streak++;

                LastScoreChange = upvotedSong.Streak * GetUpvoteWeight(upvotedSong.Score) * (float)percentage * 8;
                upvotedSong.Score += upvotedSong.Streak * GetUpvoteWeight(upvotedSong.Score) * (float)percentage * 8;
                LastUpvotedSongStreak = upvotedSong.Streak;

                upvotedSong.TotalLikes++;

                UpdateSongChoosingList(currentlyPlayingSongPath);
            }
            IsCurrentSongUpvoted = false;
        }
        public static void RegisterNewSong(string songPath)
        {
            Playlist.Add(songPath);
            AddSongToListIfNotDoneSoFar(songPath);
            UpdateSongDate(songPath);
        }
        public static void AddSongToListIfNotDoneSoFar(string Song)
        {
            var songName = Song.Split('\\').Last();
            if (DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == songName) == null)
                DbHolder.DbContext.UpvotedSongs.Add(new UpvotedSong(Song.Split('\\').Last(), 0, 0, 0, 0, GetSongFileCreationDate(Song), -1));
        }
        public static void SaveUserSettings(bool SongSwap)
        {
            if (SongSwap)
            {
                UpvoteCurrentSongIfNeccesary();
                SaveCurrentSongToHistoryFile(LastScoreChange);
                LastScoreChange = 0;
            }

            DbHolder.DbContext.SaveChanges();

            Task.Run(() =>
            {
                if (!SavingToFileRightNow)
                {
                    SavingToFileRightNow = true;

                    lock (Config.Data)
                    {
                        Config.Data.Background = (int)Program.game.BgModes;
                        Config.Data.Vis = (int)Program.game.VisSetting;
                        Config.Data.Col = System.Drawing.Color.FromArgb(Program.game.primaryColor.R, Program.game.primaryColor.G, Program.game.primaryColor.B);
                        Config.Data.firstStart = false;
                        Config.Save();
                    }

                    SavingToFileRightNow = false;
                }
            });
        }
        public static bool DoesCurrentSongaveNoVolumeData()
        {
            if (currentlyPlayingSongData != null)
                return currentlyPlayingSongData.Volume == -1;
            return false;
        }
        public static List<HistorySong> LoadSongHistoryFile(string path, int maxEntries)
        {
            if (!File.Exists(path))
                return new List<HistorySong>();

            string[] Songs = File.ReadLines(path).ToArray();

            List<HistorySong> re = new();
            for (int i = 0; i < Math.Min(Songs.Length, maxEntries); i++)
            {
                string[] Split = Songs[Songs.Length - i - 1].Split(':');
                string Title = "";
                long Time = 0;
                float ScoreChange = 0;
                if (Split.Length == 3)
                {
                    Time = Convert.ToInt64(Split[0].Trim('\t'));
                    ScoreChange = Convert.ToSingle(Split[1].Trim('\t'));
                    Title = Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(Split[2].Trim('\t')));
                }

                re.Add(new HistorySong(Title, ScoreChange, Time));
            }
            return re;
        }

        // For Statistics
        public static long GetSongFileCreationDate(string SongPath)
        {
            if (File.Exists(SongPath))
                return File.GetCreationTime(SongPath).ToBinary();
            else
                return 0;
        }
        public static void UpdateSongDate(string SongPath)
        {
            string songName = SongPath.Split('\\').LastOrDefault();
            var upvotedSong = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(predicate: x => x.Name == songName);
            if (upvotedSong == null)
                return;
            long OriginalSongBinary = upvotedSong.AddingDates;
            DateTime OriginalSongCreationDate = DateTime.FromBinary(OriginalSongBinary);
            if (OriginalSongBinary == 0 || File.Exists(SongPath) && DateTime.Compare(OriginalSongCreationDate, File.GetCreationTime(SongPath)) > 0)
                upvotedSong.AddingDates = File.GetCreationTime(SongPath).ToBinary();
        }
        private static float SongAge(UpvotedSong upvotedSong)
        {
            return (float)Math.Round(DateTime.Today.Subtract(DateTime.FromBinary(upvotedSong.AddingDates)).TotalHours / 24.0, 4) + 1f;
        }
        public static float SongAge(string SongPath)
        {
            string songName = SongPath.Split('\\').LastOrDefault();
            if (File.Exists(SongPath) && songName != null)
                return SongAge(DbHolder.DbContext.UpvotedSongs.FirstOrDefault(x => x.Name == songName));
            else
                return float.NaN;
        }
        public static object[,] GetSongInformationList()
        {
            int songsAmount = DbHolder.DbContext.UpvotedSongs.Count();
            object[,] SongInformationArray = new object[songsAmount, 7];

            foreach (var item in Enumerable.Range(0, songsAmount).Zip(DbHolder.DbContext.UpvotedSongs.OrderByDescending(x => x.Score)))
            {
                var (i, curSong) = item;

                string title = curSong.Name;
                while (title.EndsWith(".mp3"))
                    title = title.Remove(title.Length - 4);

                SongInformationArray[i, 0] = title;
                SongInformationArray[i, 1] = curSong.Score;
                SongInformationArray[i, 2] = curSong.Streak;
                SongInformationArray[i, 3] = curSong.TotalLikes + "/" + curSong.TotalDislikes + "=" + ((float)curSong.TotalLikes / curSong.TotalDislikes);
                if (curSong.Volume != -1)
                    SongInformationArray[i, 4] = Values.BaseVolume / curSong.Volume;
                SongInformationArray[i, 5] = SongAge(curSong);
                SongInformationArray[i, 6] = SongChoosingList.FindAll(x => x == curSong.Path).Count / (float)SongChoosingList.Count * 100;
            }

            return SongInformationArray;
        }
        public static string GetSongPathFromSongName(string SongName)
        {
            if (!SongName.EndsWith(".mp3"))
                SongName += ".mp3";

            foreach (string s in Playlist)
                if (s.Split('\\').Last() == SongName && File.Exists(s))
                    return s;
            return "";
        }
        private static void SaveCurrentSongToHistoryFile(float ScoreChange)
        {
            try { string s = currentlyPlayingSongName; } catch { return; }
            if (HistorySongData.Count > 0 && currentlyPlayingSongName == HistorySongData[0].Name)
                return;

            HistorySongData.Add(new HistorySong(Path.GetFileNameWithoutExtension(currentlyPlayingSongName), ScoreChange, DateTime.Now.ToBinary()));
            var newEntry = HistorySongData.Last();
            File.AppendAllText(SongManager.historyFilePath, $"{newEntry.Date}\t:{newEntry.Change}\t\t\t:{Uri.EscapeDataString(newEntry.Name)}\n");

            //HistorySongData = HistorySongData.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
        }
        private static void TestChoosingListIntegrity()
        {
            //for (int i = 0; i < Config.Data.songDatabaseEntries.Count; i++)
            //{
            //    float count = SongChoosingList.FindAll(x => x == Config.Data.songDatabaseEntries[i].Path).Count;
            //    float target = GetSongChoosingAmount(i) + 1;

            //    if (Math.Abs(count - target) > 1 && Config.Data.songDatabaseEntries[i].Path != null)
            //        Config.Data.songDatabaseEntries.GetHashCode(); // Breakpoint here
            //}
        }

        // Dump
        public static void DisposeNAudioData()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (Channel32 != null)
            {
                try
                {
                    Channel32.Dispose();
                    Channel32 = null;
                }
                catch { }
            }
            if (Channel32Reader != null)
            {
                try
                {
                    Channel32Reader.Dispose();
                }
                catch { Debug.WriteLine("Couldn't dispose the reader"); }
                Channel32Reader = null;
            }
            if (Channel32ReaderThreaded != null)
            {
                try
                {
                    Channel32ReaderThreaded.Dispose();
                }
                catch { Debug.WriteLine("Couldn't dispose the reader"); }
                Channel32ReaderThreaded = null;
            }
            if (mp3 != null)
            {
                mp3.Dispose();
                mp3 = null;
            }
        }
    }
}
