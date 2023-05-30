using Configuration;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
    public static class ConsoleManager
    {
        public static bool BackgroundOperationRunning = false;
        public static bool ConsoleBackgroundOperationRunning = false;
        public static bool PauseConsoleInputThread = false;
        static Task ConsoleManagerTask;
        static Thread ConsoleManagerThread;
        static int originY;
        static int LastConsoleInputIndex = -1;
        static List<string> LastConsoleInput = new List<string>();
        public static string wakeUpChar = "Ԫ";

        public static void StartSongInputLoop()
        {
            ConsoleManagerTask = Task.Factory.StartNew(() =>
            {
                ConsoleManagerThread = Thread.CurrentThread;
                ConsoleManagerThread.Name = "Console Manager Thread";
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    string Path = "";
                    originY = Console.CursorTop;
                    while (!Path.Contains(".mp3\""))
                    {
                        Path = Path.Replace(wakeUpChar, "");

                        //Thread.Sleep(5);
                        Console.SetCursorPosition(0, originY);
                        for (int i = 0; i < Path.Length / 65 + 4; i++)
                            Console.Write("                                                                    ");
                        Console.SetCursorPosition(0, originY);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Play Song: ");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(Path);

                        ConsoleKeyInfo e = Console.ReadKey();

                        if (Program.closing)
                            break;

                        if (PauseConsoleInputThread) { Console.CursorLeft = 0; }
                        while (PauseConsoleInputThread) { Thread.Sleep(50); }

                        if (e.Key == ConsoleKey.UpArrow)
                        {
                            LastConsoleInputIndex++;
                            if (LastConsoleInputIndex > LastConsoleInput.Count - 1)
                                LastConsoleInputIndex = LastConsoleInput.Count - 1;
                            if (LastConsoleInput.Count > 0)
                                Path = LastConsoleInput[LastConsoleInput.Count - 1 - LastConsoleInputIndex];
                        }

                        if (e.Key == ConsoleKey.DownArrow)
                        {
                            LastConsoleInputIndex--;
                            if (LastConsoleInputIndex < -1)
                                LastConsoleInputIndex = -1;
                            if (LastConsoleInputIndex == -1)
                                Path = "";
                            if (LastConsoleInputIndex > -1)
                                Path = LastConsoleInput[LastConsoleInput.Count - 1 - LastConsoleInputIndex];
                        }

                        if (e.Key == ConsoleKey.Enter)
                        {
                            #region ConsoleCommands
                            if (Path.StartsWith("/"))
                            {
                                if (Path == "/cls")
                                {
                                    LastConsoleInput.Add(Path);
                                    Path = "";
                                    Console.Clear();
                                    originY = 0;
                                }
                                else if (Path == "/f")
                                {
                                    LastConsoleInput.Add(Path);
                                    Path = "";
                                    Program.game.gameWindowForm.InvokeIfRequired(() => { Program.game.gameWindowForm.BringToFront(); Program.game.gameWindowForm.Activate(); });
                                    originY++;
                                }
                                else if (Path == "/s")
                                {
                                    Console.WriteLine("Target Volume: " + Values.TargetVolume + ", Output Volume: " + Values.OutputVolume);
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path == "/t" || Path == "/time")
                                {
                                    LastConsoleInput.Add(Path);
                                    originY++;
                                    Path = "";
                                    Console.SetCursorPosition(0, originY);
                                    Console.WriteLine(Values.AsTime((SongManager.Channel32.Position / (double)SongManager.Channel32.Length) * SongManager.Channel32.TotalTime.TotalSeconds)
                                        + " / " + Values.AsTime(SongManager.Channel32.TotalTime.TotalSeconds));
                                    originY++;
                                }
                                else if (Path.StartsWith("/settime "))
                                {
                                    LastConsoleInput.Add(Path);
                                    Path = Path.Remove(0, "/settime ".Length);

                                    try
                                    {
                                        SongManager.Channel32.Position = (long)(Convert.ToInt32(Path) * (SongManager.Channel32.Length / SongManager.Channel32.TotalTime.TotalSeconds));
                                    }
                                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }

                                    Path = "";
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path.StartsWith("/songbufferstate"))
                                {
                                    Console.WriteLine("\nWas aborted: " + SongVisualization.SongBufferThreadWasAborted);
                                    Console.WriteLine("\nLast exception: " + SongVisualization.LastSongBufferThreadException.ToString());
                                    Console.WriteLine("\nLast exception stack trace: " + SongVisualization.LastSongBufferThreadException.StackTrace + "\n");

                                    Path = "";
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path.StartsWith("/stack"))
                                {
                                    Console.CursorTop++;
                                    try
                                    {
                                        Program.game.MainThread.Suspend();
                                        //Console.WriteLine(new StackTrace(MainThread, true));
                                        Console.WriteLine("idk");
                                    }
                                    catch { }
                                    finally { Program.game.MainThread.Resume(); }

                                    Path = "";
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path.StartsWith("/stopBufferLoading"))
                                {
                                    Console.CursorTop++;

                                    Program.game.SongBufferLoaderThread.Abort();

                                    Path = "";
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path.StartsWith("yt-dlp -U"))
                                {
                                    "yt-dlp -U".RunAsConsoleCommand(360, () => { }, (string o, string err) => { Console.Write(o + e); });
                                }
                                else if (Path == "/showinweb" || Path == "/showinnet" || Path == "/net" || Path == "/web")
                                {
                                    LastConsoleInput.Add(Path);
                                    originY++;
                                    Path = "";
                                    Console.SetCursorPosition(0, originY);
                                    Console.WriteLine("Searching for " + SongManager.currentlyPlayingSongName.Split('.').First() + "...");
                                    originY++;

                                    Task.Factory.StartNew(() =>
                                    {
                                        // Use the I'm Feeling Lucky URL
                                        string url = string.Format("https://www.google.com/search?num=100&site=&source=hp&q={0}&btnI=1", SongManager.currentlyPlayingSongName.Split('.').First());
                                        url = url.Replace(' ', '+');
                                        WebRequest req = HttpWebRequest.Create(url);
                                        Uri U = req.GetResponse().ResponseUri;

                                        Process.Start(U.ToString());
                                    });
                                }
                                else if (Path == "/help")
                                {
                                    LastConsoleInput.Add(Path);
                                    Path = "";
                                    Console.WriteLine();
                                    Console.WriteLine("All currently implemented cammands: ");
                                    Console.WriteLine();
                                    Console.WriteLine("/f - focuses the main window");
                                    Console.WriteLine();
                                    Console.WriteLine("/cls - clears the console");
                                    Console.WriteLine();
                                    Console.WriteLine("/download | /d | /D - Searches for the current song on youtube, converts it to  mp3 and puts it into the standard folder");
                                    Console.WriteLine();
                                    Console.WriteLine("/showinweb | /showinnet | /net | /web - will search google for the current      songs name and display the first result in the standard browser");
                                    Console.WriteLine();
                                    Console.WriteLine("/queue | /q - adds a song to the queue");
                                    Console.WriteLine();
                                    Console.WriteLine("/time | /t - shows the current song play time");
                                    Console.WriteLine();
                                    Console.WriteLine("/settime - sets the current play time");
                                    Console.WriteLine();
                                    Console.WriteLine("/s - shows volumes");
                                    Console.WriteLine();
                                    originY = Console.CursorTop;
                                }
                                else if (Path.StartsWith("/d") || Path.StartsWith("/D") || Path.StartsWith("https://www.youtube.com/watch?v="))
                                {
                                    try
                                    {
                                        Console.WriteLine();
                                        LastConsoleInput.Add(Path);
                                        string download;
                                        if (Path.StartsWith("/download"))
                                            download = Path.Remove(0, "/download".Length + 1);
                                        else if (!Path.StartsWith("https://www.youtube.com/watch?v="))
                                            download = Path.Remove(0, "/d".Length + 1);
                                        else
                                            download = Path;

                                        Download(download);
                                        Path = "";
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        BackgroundOperationRunning = false;
                                    }

                                    originY = Console.CursorTop;
                                }
                                else if (Path.StartsWith("/q"))
                                {
                                    try
                                    {
                                        Console.WriteLine();
                                        LastConsoleInput.Add(Path);
                                        string queue;
                                        if (Path.StartsWith("/queue"))
                                            queue = Path.Remove(0, "/queue".Length + 1);
                                        else
                                            queue = Path.Remove(0, "/q".Length + 1);
                                        SongManager.QueueNewSong(Path, true);
                                        Path = "";
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }

                                    originY = Console.CursorTop;
                                }
                                else
                                {
                                    Console.WriteLine("I dont know that comamnd");
                                    originY = Console.CursorTop;
                                }
                            }
                            else
                                break;
                            #endregion

                            LastConsoleInputIndex = -1;
                        }

                        if (e.Key == ConsoleKey.Backspace)
                        {
                            if (Path.Length > 0)
                                Path = Path.Remove(Path.Length - 1);
                        }
                        else if (e.Key >= (ConsoleKey)48 && e.Key <= (ConsoleKey)90 ||
                                 e.Key >= (ConsoleKey)186 && e.Key <= (ConsoleKey)226 ||
                                 e.Key == ConsoleKey.Spacebar)
                            Path += e.KeyChar;
                    }
                    Console.WriteLine();
                    if (Path.StartsWith("https://"))
                        Download(Path);
                    else if (SongManager.PlayNewSong(Path))
                        LastConsoleInput.Add(Path.Trim('"'));
                }
            });
        }
        public static void HandleCancelKeyPress(object o, ConsoleCancelEventArgs e)
        {
            if (!Program.closing)
            {
                e.Cancel = true;
                Console.ForegroundColor = ConsoleColor.Red;
                if (Console.CursorLeft != 0)
                {
                    Console.WriteLine();
                }
                Console.WriteLine("Canceled by user!");
                PauseConsoleInputThread = false;
                ConsoleBackgroundOperationRunning = false;
                originY = Console.CursorTop;
            }
        }
        public static bool IsTaskClosed()
            => ConsoleManagerTask.IsCanceled || ConsoleManagerTask.IsCompleted || ConsoleManagerTask.IsFaulted;

        /// <summary>
        /// Downloads a song using ytdlp
        /// </summary>
        /// <param name="downloadInput"></param>
        /// <returns>Number of songs that have been downloaded</returns>
        public static int Download(string downloadInput)
        {
            if (BackgroundOperationRunning || ConsoleBackgroundOperationRunning)
            {
                MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                return 0;
            }

            try
            {
                ConsoleBackgroundOperationRunning = true;
                PauseConsoleInputThread = true;
                Values.ShowConsole();

                PlaybackState PlayState = SongManager.output.PlaybackState;

                string downloadTargetFolder = $"{Values.CurrentExecutablePath}\\Downloads";
                string download = downloadInput;
                if (!download.StartsWith("https://"))
                    download = $"\"ytsearch: {downloadInput}\"";

                Console.ForegroundColor = ConsoleColor.Yellow;

                string output = $"\"%(title)s.%(ext)s\"";
                if ((download.Contains("ytsearch") ||
                    download.Contains("https://www.youtube.com")) && !downloadInput.GetYoutubeVideoTitle().Contains(" - "))
                    output = $"\"%(uploader)s - %(title)s.%(ext)s\"";

                // Download Video File
                Process P = new Process();
                P.StartInfo = new ProcessStartInfo("yt-dlp.exe", download + $" --split-chapters -x --audio-format mp3 -P \"{downloadTargetFolder}\" -o {output} --add-metadata --embed-thumbnail");
                P.StartInfo.UseShellExecute = false;
                P.Start();
                P.WaitForExit();

                // move files to lib
                var downloadedSongsPaths = new List<string>();
                foreach (string musicFilepath in Directory.GetFiles(downloadTargetFolder).Where(x => x.EndsWith(".mp3")))
                {
                    string musicFile = Path.GetFileName(musicFilepath);
                    string targetPath = $"{Config.Data.MusicPath}\\{musicFile.Replace(" - Topic", "")}";
                    // Override
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    File.Move(musicFilepath, targetPath);

                    SongManager.RegisterNewSong(targetPath);

                    downloadedSongsPaths.Add(targetPath);
                }


                foreach (var file in Directory.GetFiles(downloadTargetFolder))
                    File.Delete(file);

                if (string.IsNullOrEmpty(downloadedSongsPaths.First()))
                    return 0;

                // Play it
                SongManager.PlayNewSong(downloadedSongsPaths.First());
                originY = Console.CursorTop;

                if (PlayState == PlaybackState.Paused || PlayState == PlaybackState.Stopped)
                    SongManager.PlayPause();

                ConsoleBackgroundOperationRunning = false;
                PauseConsoleInputThread = false;

                //CreateGlobalKeyHooks();

                return downloadedSongsPaths.Count;
            }
            catch (Exception e)
            {
                ConsoleBackgroundOperationRunning = false;
                PauseConsoleInputThread = false;

                MessageBox.Show(e.ToString());
                return 0;
            }
        }
        public static bool DownloadAsVideo(string url)
        {
            if (BackgroundOperationRunning || ConsoleBackgroundOperationRunning)
            {
                MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                return false;
            }

            if (Config.Data.BrowserDownloadFolderPath == "" || Config.Data.BrowserDownloadFolderPath == null)
            {
                MessageBox.Show("I need to put it into the BrowserDownloadFolderPath but I dont have it");
                return false;
            }

            try
            {
                ConsoleBackgroundOperationRunning = true;
                PauseConsoleInputThread = true;
                Values.ShowConsole();

                Console.ForegroundColor = ConsoleColor.Yellow;

                Process P = new Process();
                P.StartInfo = new ProcessStartInfo("yt-dlp.exe", $"-f mp4 -o \"{Config.Data.BrowserDownloadFolderPath}\\%(title)s.%(ext)s\" {url}");
                P.StartInfo.UseShellExecute = false;

                P.Start();
                P.WaitForExit();

                originY = Console.CursorTop;

                ConsoleBackgroundOperationRunning = false;
                PauseConsoleInputThread = false;

                KeyHookManager.CreateGlobalKeyHooks(Program.game.gameWindowForm.Handle);
            }
            catch (Exception e)
            {
                ConsoleBackgroundOperationRunning = false;
                PauseConsoleInputThread = false;

                MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }
    }
}
