using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Configuration;
using Configuration;
using System.Text.Encodings.Web;

namespace MusicPlayerDXMonoGamePort
{
    public static class Program
    {
        public static string[] args;
        public static XNA game;
        public static bool closing = false;
        public static FileSystemWatcher weightwatchers;
        public static FileSystemWatcher crackopenthebois;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "MusicPlayer Console";

            CheckForOtherInstances();

            // Smol settings
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Values.DisableConsoleRezise();
            Values.RegisterUriScheme();

            InitSongDataList();

            Console.Clear();

            // Actual start
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Created with \"MonoGame\" and \"NAudio\"");
            Console.WriteLine("Build from: " + File.ReadAllText(Values.CurrentExecutablePath + "\\BuildDate.txt").TrimEnd('\n'));

            Program.args = args;

            if (Config.Data.DiscordRpcActive)
                DiscordRPCWrapper.Initialize("460490126607384576");

            ClearOldBrowserReqs();

            CreateFilewatchers();

#if DEBUG
            game = new XNA();
            game.Run();
#else
            try
            {
                game = new XNA();
                game.Run();
            }
            catch (Exception ex)
            {
                HandleFatalProductionError(ex);
            }
#endif
        }

        public static void HandleFatalProductionError(Exception ex)
        {
            string strPath = Values.CurrentExecutablePath + @"\Log.txt";
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine();
                sw.WriteLine("==========================Error Logging========================");
                sw.WriteLine("============Start=============" + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine("=============End=============");
            }

            DiscordRPCWrapper.Shutdown();
            KeyHookManager.DisposeGlobalKeyHooks();
            SongManager.DisposeNAudioData();
            if (game.optionsMenu != null)
                game.optionsMenu.InvokeIfRequired(game.optionsMenu.Close);
            if (game.statistics != null)
                game.statistics.InvokeIfRequired(game.statistics.Close);
            closing = true;

            DialogResult D;
            if (ex.Message.Contains("WindowsGameForm"))
                D = MessageBox.Show("I got brutally murdered by another Program. Please restart me.", "Slaughtered by another program",
                    MessageBoxButtons.RetryCancel);
            else if (ex.Message == "CouldntFindWallpaperFile")
                D = MessageBox.Show("You seem to have moved your Desktop Wallpaper file since you last set it as your Wallpaper.\n" +
                    "Please set it as your wallpaper again and restart me so I can actually find its file.",
                    "Couldn't find your wallpaper", MessageBoxButtons.RetryCancel);
            else
                D = MessageBox.Show("Error Message: " + ex.Message + "\n\nStack Trace: \n" + ex.StackTrace +
                    "\n\nInner Error: " + ex.InnerException + "\n\nSource: " + ex.Source, "Error", MessageBoxButtons.RetryCancel);

            if (D == DialogResult.Retry)
                Restart();
        }
        public static void CreateFilewatchers()
        {
            // SettingsPath
            weightwatchers = new FileSystemWatcher();
            try
            {
                string[] P = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\MusicPlayer");
#if DEBUG
                string SettingsPath = P[1] + @"\1.0.0.0";
#else
                string SettingsPath = P[0] + @"\1.0.0.0";
#endif
                if (Directory.Exists(SettingsPath))
                {
                    weightwatchers.Path = SettingsPath;
                    weightwatchers.Changed += ((object source, FileSystemEventArgs e) =>
                    {
                        try
                        {
                            game.CheckForRequestedSongs();
                        }
                        catch { }
                    });
                    weightwatchers.EnableRaisingEvents = true;
                }
                else
                {
                    Console.WriteLine("Couldn't set filewatcher! (WRONG SETTINGSPATH: " + SettingsPath + " )");
                }
            }
            catch { Console.WriteLine("Couldn't set filewatcher! (UNKNOWN ERROR)"); }

            // DownloadPath
            if (Config.Data.BrowserDownloadFolderPath != "" && Config.Data.BrowserDownloadFolderPath != null)
            {
                crackopenthebois = new FileSystemWatcher();
                try
                {
                    if (Directory.Exists(Config.Data.BrowserDownloadFolderPath))
                    {
                        Config.Data.BrowserDownloadFolderPath = Config.Data.BrowserDownloadFolderPath;
                        Config.Save();

                        crackopenthebois.Path = Config.Data.BrowserDownloadFolderPath;
                        crackopenthebois.Changed += CrackOpen;
                        crackopenthebois.EnableRaisingEvents = true;
                    }
                    else
                    {
                        MessageBox.Show("Couldn't set filewatcher! (wrong SelectedPath: " + Config.Data.BrowserDownloadFolderPath + " )");
                    }
                }
                catch (Exception ex) { MessageBox.Show("Couldn't set filewatcher! (ERROR: " + ex + ")"); }
            }
        }
        public static void ClearOldBrowserReqs()
        {
            if (Config.Data.BrowserDownloadFolderPath != "" && Config.Data.BrowserDownloadFolderPath != null)
            {
                string[] bois = Directory.GetFiles(Config.Data.BrowserDownloadFolderPath);
                for (int i = 0; i < bois.Length; i++)
                {
                    string fileExtension = Path.GetExtension(bois[i]);
                    if (fileExtension == ".PlayRequest")
                        File.Delete(bois[i]);
                    if (fileExtension == ".VideoDownloadRequest")
                        File.Delete(bois[i]);
                }
            }
        }
        public static void InitSongDataList()
        {
            // Legacy config support
            if (config.Default.SongPaths != null && config.Default.SongScores != null && config.Default.SongUpvoteStreak != null && config.Default.SongTotalLikes != null &&
                config.Default.SongTotalDislikes != null && config.Default.SongDate != null && config.Default.SongVolume != null &&
                config.Default.SongScores.Length == config.Default.SongPaths.Length && config.Default.SongUpvoteStreak.Length == config.Default.SongPaths.Length &&
                config.Default.SongTotalLikes.Length == config.Default.SongPaths.Length && config.Default.SongTotalDislikes.Length == config.Default.SongPaths.Length &&
                config.Default.SongDate.Length == config.Default.SongPaths.Length && config.Default.SongVolume.Length == config.Default.SongPaths.Length)
            {
                Config.Data.songDatabaseEntries.Clear();
                for (int i = 0; i < config.Default.SongPaths.Length; i++)
                    Config.Data.songDatabaseEntries.Add(new UpvotedSong(config.Default.SongPaths[i], config.Default.SongScores[i], config.Default.SongUpvoteStreak[i],
                            config.Default.SongTotalLikes[i], config.Default.SongTotalDislikes[i], config.Default.SongDate[i], config.Default.SongVolume[i]));
                config.Default.SongPaths = null;
                config.Default.Save();
            }

            SongManager.HistorySongData = SongManager.LoadSongHistoryFile(SongManager.historyFilePath, 25);
        }
        public static void CheckForOtherInstances()
        {
            Console.WriteLine("Checking for other MusicPlayer instances...");
            try
            {
                foreach (Process p in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                    if (p.Id != Process.GetCurrentProcess().Id && p.MainModule.FileName == Process.GetCurrentProcess().MainModule.FileName)
                    {
                        Console.WriteLine("Found another instance. \nSending data...");
                        if (args.Length > 0)
                        {
                            RequestedSong.Default.RequestedSongString = args[0];
                            RequestedSong.Default.Save();
                        }
                        Console.WriteLine("Data sent! Closing...");
                        return;
                    }
            }
            catch
            {
                Console.WriteLine("Please just start one instance of me at a time!");
                Thread.Sleep(1000);
                return;
            }
            // Also check for cheeky curious changes to the settings
            if (Config.Data.MultiThreading == false)
            {
                MessageBox.Show("Dont mess with the settings file!\nLook this is an old option and it wont do much but possibly break the program so just activate it again.");
                return;
            }
        }
        public static void Restart()
        {
            string RestartLocation = Values.CurrentExecutablePath + "\\Restart.bat";

            if (File.Exists(RestartLocation))
                File.Delete(RestartLocation);

            using (StreamWriter sw = File.CreateText(RestartLocation))
                sw.WriteLine(@"@echo off
echo     ____               __                __   _                     
echo    / __ \ ___   _____ / /_ ____ _ _____ / /_ (_)____   ____ _       
echo   / /_/ // _ \ / ___// __// __ `// ___// __// // __ \ / __ `/       
echo  / _, _//  __/(__  )/ /_ / /_/ // /   / /_ / // / / // /_/ /_  _  _ 
echo /_/ ^|_^| ^\___//____/ \__/ \__,_//_/    \__//_//_/ /_/ \__, /(_)(_)(_)
echo                                                     /____/          
ping 127.0.0.1 > nul
start MusicPlayerDXMonoGamePort.exe");

            Process.Start(RestartLocation);
        }
        #region Event Handlers
        static object crackLock = new object();
        public static void CrackOpen(object source, FileSystemEventArgs ev)
        {
            Thread.Sleep(50);
            string[] bois = Directory.GetFiles(Config.Data.BrowserDownloadFolderPath);
            for (int i = 0; i < bois.Length; i++)
            {
                string fileName = Path.GetFileName(bois[i]);

                if (fileName == "MusicPlayer.PlayRequest" || fileName == "MusicPlayer.VideoDownloadRequest")
                {
                    string crackedOpenBoi = File.ReadAllText(bois[i]);
                    Task.Factory.StartNew(() =>
                    {
                        lock (crackLock)
                        {
                            while (ConsoleManager.BackgroundOperationRunning || ConsoleManager.ConsoleBackgroundOperationRunning)
                                Thread.Sleep(250);
                            
                            if (fileName == "MusicPlayer.PlayRequest")
                            {
                                string[] split = crackedOpenBoi.Split('±');
                                if (ConsoleManager.Download(split[0]) == 1 && split.Length > 1)
                                {
                                    long secondspassed = Convert.ToInt64(split[1].Split('.')[0]);
                                    SongManager.Channel32.Position = secondspassed * SongManager.Channel32.WaveFormat.AverageBytesPerSecond;
                                }
                            }
                            if (fileName == "MusicPlayer.VideoDownloadRequest")
                                ConsoleManager.DownloadAsVideo(crackedOpenBoi);
                        }
                    });
                    File.Delete(bois[i]);
                    Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                    Values.SetForegroundWindow(Values.GetConsoleWindow());
                    SendKeys.SendWait(ConsoleManager.wakeUpChar);
                }
            }
        }
        #endregion
    }
}

