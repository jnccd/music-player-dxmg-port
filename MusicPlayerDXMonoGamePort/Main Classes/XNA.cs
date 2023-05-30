﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using NAudio.Wave;
using NAudio.Dsp;
using System.IO;
using System.Net;
using MediaToolkit.Model;
using MediaToolkit;
using System.Runtime;
using MessageBox = System.Windows.Forms.MessageBox;
using SharpDX.Direct2D1.Effects;
using Configuration;
using System.Reflection.Emit;
using Keys = System.Windows.Forms.Keys;
using RawInput_dll;

namespace MusicPlayerDXMonoGamePort
{
    public enum Visualizations
    {
        line,
        dynamicline,
        fft,
        rawfft,
        barchart,
        grid,
        trumpetboy,
        none
    }
    public enum BackGroundModes
    {
        None,
        Blur,
        coverPic,
        BlurVignette,
        BlurTeint
    }
    public enum SelectedControl
    {
        VolumeSlider,
        DurationBar,
        DragWindow,
        PlayPauseButton,
        UpvoteButton,
        OptionsButton,
        None,
        CloseButton
    }

    public class XNA : Game
    {
        // Graphics
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public Form gameWindowForm;
        public RenderTarget2D TempBlur;
        public RenderTarget2D BlurredTex;
        public RenderTarget2D TitleTarget;
        public RenderTarget2D BackgroundTarget;
        const float abstand = 35;
        const float startX = 10;
        const float speed = 0.35f;

        // Visualization
        public Visualizations VisSetting = (Visualizations)Config.Data.Vis;
        public BackGroundModes BgModes = (BackGroundModes)Config.Data.Background;
        public Color primaryColor = Color.FromNonPremultiplied(25, 75, 255, 255);
        public Color secondaryColor = Color.Green;
        public Color backgroundColor = Color.White;
        public GaussianDiagram GauD = null;
        public DynamicGrid DG;
        public ColorDialog LeDialog;
        public bool ShowingColorDialog = false;
        DropShadow Shadow;
        bool LongTitle;

        // Stuff
        System.Drawing.Point MouseClickedPos = new System.Drawing.Point();
        System.Drawing.Point WindowLocation = new System.Drawing.Point();
        SelectedControl selectedControl = SelectedControl.None;
        float SecondRowMessageAlpha;
        string SecondRowMessageText;
        public float UpvoteSavedAlpha = 0;
        float UpvoteIconAlpha = 0;
        List<string> LastConsoleInput = new List<string>();
        int LastConsoleInputIndex = -1;
        List<float> DebugPercentages = new List<float>();
        public bool FocusWindow = false;
        public bool Preload;
        int originY;
        float[] values;
        bool WasFocusedLastFrame = true;
        public bool BackgroundOperationRunning = false;
        public bool ConsoleBackgroundOperationRunning = false;
        public bool PauseConsoleInputThread = false;
        public Task ConsoleManager;
        Thread ConsoleManagerThread;
        public Thread SongBufferLoaderThread;
        Thread MainThread;
        Task SongCheckThread;
        Task CloseConfirmationThread;
        const float MaxVolume = 1f;
        int lastSongRequestCheck = -100;
        public long SkipStartPosition = 0;
        public long SongTimeSkipped = 0;
        string lastQuestionResult = null;
        bool ForcedTitleRedraw = false;
        bool ForcedBackgroundRedraw = false;
        public bool ForcedCoverBackgroundRedraw = false;
        System.Drawing.Point newPos = new System.Drawing.Point(Config.Data.WindowPos.X, Config.Data.WindowPos.Y);
        System.Drawing.Point oldPos;
        Point Diff = new Point();
        Point[] WindowPoints = new Point[4];
        string Title;
        float X1;
        float X2;
        int ScrollWheelCooldown = 0;
        bool wasClickedOn = false;
        public static KeyboardHook keyHook = null;
        private RawInput _rawinput = null;

        public OptionsMenu optionsMenu;
        public Statistics statistics;
        public History history;
        
        // Draw
        Vector2 TempVector = new Vector2(0, 0);
        Rectangle TempRect = new Rectangle(0, 0, 0, 0);
        Rectangle DurationBar = new Rectangle(51, Values.WindowSize.Y - 28, Values.WindowSize.X - 157, 3);
        Rectangle VolumeIcon = new Rectangle(Values.WindowSize.X - 132, 16, 24, 24);
        Rectangle VolumeBar = new Rectangle(Values.WindowSize.X - 100, 24, 75, 8);
        Rectangle PlayPauseButton = new Rectangle(24, Values.WindowSize.Y - 34, 16, 16);
        Rectangle Upvote = new Rectangle(24, 43, 20, 20);
        Rectangle TargetVolumeBar = new Rectangle(); // needs Updates
        Rectangle ActualVolumeBar = new Rectangle(); // needs Updates
        Rectangle UpvoteButton = new Rectangle(Values.WindowSize.X - 98, Values.WindowSize.Y - 35, 19, 19);
        Rectangle CloseButton = new Rectangle(Values.WindowSize.X - 43, Values.WindowSize.Y - 34, 18, 18);
        Rectangle OptionsButton = new Rectangle(Values.WindowSize.X - 71, Values.WindowSize.Y - 34, 19, 19);

        // Shadows
        Rectangle DurationBarShadow;
        Rectangle VolumeIconShadow;
        Rectangle VolumeBarShadow;
        Rectangle PlayPauseButtonShadow;
        Rectangle UpvoteShadow;
        Rectangle UpvoteButtonShadow;
        Rectangle CloseButtonShadow;
        Rectangle OptionsButtonShadow;

        // Hitbox Rectangles
        Rectangle DurationBarHitbox;
        Rectangle VolumeBarHitbox;
        Rectangle PlayPauseButtonHitbox;
        Rectangle UpvoteButtonHitbox;

        public XNA()
        {
            UpdateShadowRects();

            DurationBarHitbox = new Rectangle(DurationBar.X, DurationBar.Y - 10, DurationBar.Width, 23);
            VolumeBarHitbox = new Rectangle(Values.WindowSize.X - 100, 20, 110, 16);
            PlayPauseButtonHitbox = new Rectangle(14, Values.WindowSize.Y - 39, 26, 26);
            UpvoteButtonHitbox = new Rectangle(UpvoteButton.X, UpvoteButton.Y, 20, 20);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = Values.WindowSize.X;
            graphics.PreferredBackBufferHeight = Values.WindowSize.Y;
            graphics.PreferHalfPixelOffset = true;
            gameWindowForm = (Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            gameWindowForm.FormBorderStyle = FormBorderStyle.None;
            gameWindowForm.Move += ((object sender, EventArgs e) =>
            {
                gameWindowForm.Location = new System.Drawing.Point(Config.Data.WindowPos.X, Config.Data.WindowPos.Y);
            });
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 120.0f);
            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            MainThread = Thread.CurrentThread;
            optionsMenu = new OptionsMenu(this);
            IsMouseVisible = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
            values = new float[Values.WindowSize.X - 70];
            for (int i = 0; i < values.Length; i++)
                values[i] = 0;
            GauD = new GaussianDiagram(values, new Point(35, (int)(Values.WindowSize.Y - 60)), (int)(Values.WindowSize.Y - 125), true, 3, GraphicsDevice);
        }
        protected override void LoadContent()
        {
            gameWindowForm.FormClosing += (object sender, FormClosingEventArgs e) =>
            {
                Program.closing = true;
                DisposeGlobalKeyHooks();
                SongManager.DisposeNAudioData();
                SongManager.SaveUserSettings(true);
                if (optionsMenu != null)
                    optionsMenu.InvokeIfRequired(optionsMenu.Close);
                if (statistics != null)
                    statistics.InvokeIfRequired(statistics.Close);
            };
            Console.CancelKeyPress += ((object o, ConsoleCancelEventArgs e) =>
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
                    Program.game.PauseConsoleInputThread = false;
                    ConsoleBackgroundOperationRunning = false;
                    originY = Console.CursorTop;
                }
            });

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Preload = Config.Data.Preload;
            
            Assets.LoadLoadingScreen(Content, GraphicsDevice);

            Console.WriteLine("Updating youtube-dl...");
            Task.Factory.StartNew(() => 
                "yt-dlp -U".RunAsConsoleCommand(360, () => { }, (string o, string err) => 
                { 
                    Console.Write(o + err); 
                }));

            Assets.Load(Content, GraphicsDevice);

            BlurredTex = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);
            TempBlur = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X + 100, Values.WindowSize.Y + 100);
            Assets.gaussianBlur.Parameters["InvTexsize"].SetValue(new Vector2(1 / (float)BlurredTex.Width, 1 / (float)BlurredTex.Height));
            backgroundColor = Config.Data.BackgroundColor.ToXNAColor();

            //InactiveSleepTime = new TimeSpan(0);

            DG = new DynamicGrid(new Rectangle(35, (int)(Values.WindowSize.Y / 1.25f) - 60, Values.WindowSize.X - 70, 70), 4, 0.96f, 2.5f);
            
            Console.WriteLine("Finished Loading!");
            StartSongInputLoop();

            ShowSecondRowMessage("Found  " + SongManager.Playlist.Count + "  Songs!", 3);

            KeepWindowInScreen();
            Shadow = new DropShadow(gameWindowForm, true);
            Shadow.Show();
        }

        // Console Management
        public void StartSongInputLoop()
        {
            ConsoleManager = Task.Factory.StartNew(() =>
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
                        Path = Path.Replace("Ԫ", "");

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
                                    gameWindowForm.InvokeIfRequired(() => { gameWindowForm.BringToFront(); gameWindowForm.Activate(); });
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
                                        MainThread.Suspend();
                                        //Console.WriteLine(new StackTrace(MainThread, true));
                                        Console.WriteLine("idk");
                                    }
                                    catch { }
                                    finally { MainThread.Resume(); }

                                    Path = "";
                                    originY = Console.CursorTop + 1;
                                }
                                else if (Path.StartsWith("/stopBufferLoading"))
                                {
                                    Console.CursorTop++;

                                    SongBufferLoaderThread.Abort();

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
        /// <summary>
        /// Downloads a song using ytdlp
        /// </summary>
        /// <param name="downloadInput"></param>
        /// <returns>Number of songs that have been downloaded</returns>
        public int Download(string downloadInput)
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
        public bool DownloadAsVideo(string url)
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

                CreateGlobalKeyHooks();
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

        // Update
        protected override void Update(GameTime gameTime)
        {
            //CurrentDebugTime = Stopwatch.GetTimestamp();
            //FPSCounter.Update(gameTime);
            Control.Update();
            if (gameWindowForm.Focused)
                ComputeControls();
            if (ScrollWheelCooldown < 0)
            {
                // Next / Previous Song [MouseWheel] ((On Win10 mouseWheel input is send to the process even if its not focused))
                if (Control.ScrollWheelWentDown())
                {
                    SongManager.GetPreviousSong();
                    ScrollWheelCooldown = 60;
                }
                if (Control.ScrollWheelWentUp())
                {
                    SongManager.GetNextSong(false, true);
                    ScrollWheelCooldown = 60;
                }
            }

            if (SongManager.IsCurrentSongUpvoted) {
                if (UpvoteIconAlpha < 1)
                    UpvoteIconAlpha += 0.05f;
            }
            else if (UpvoteIconAlpha > 0)
                UpvoteIconAlpha -= 0.05f;

            Values.Timer++;
            SecondRowMessageAlpha -= 0.004f;
            UpvoteSavedAlpha -= 0.01f;
            ScrollWheelCooldown--;

            Values.LastOutputVolume = Values.OutputVolume;
            if (SongManager.output != null && SongManager.output.PlaybackState == PlaybackState.Playing)
            {
                if (LongTitle)
                    ForcedTitleRedraw = true;

                if (SongVisualization.WaveBuffer != null)
                    Values.OutputVolume = Values.GetRootMeanSquareApproximation(SongVisualization.WaveBuffer);

                if (Values.OutputVolume < 0.0001f)
                    Values.OutputVolume = 0.0001f;

                Values.OutputVolumeIncrease = Values.LastOutputVolume - Values.OutputVolume;

                if (SongManager.Channel32 != null && SongManager.Channel32.Position > SongManager.Channel32.Length - SongVisualization.bufferLength / 2)
                    SongManager.GetNextSong(false, false);

                if (Config.Data.Preload && SongVisualization.EntireSongWaveBuffer != null)
                {
                    SongVisualization.UpdateWaveBufferWithEntireSongWB();
                }
                else
                    SongVisualization.UpdateWaveBuffer();

                if (VisSetting != Visualizations.line && VisSetting != Visualizations.none && SongManager.Channel32 != null)
                {
                    SongVisualization.UpdateFFTbuffer();
                    UpdateGD();
                }
                if (VisSetting == Visualizations.grid)
                {
                    DG.ApplyForce(new Vector2(DG.Field.X + DG.Field.Width / 2, DG.Field.Y + DG.Field.Height / 2), -Values.OutputVolumeIncrease * Values.TargetVolume * 50);

                    for (int i = 1; i < DG.Points.GetLength(0) - 1; i++)
                    {
                        float InversedMagnitude = -(DG.Points[i, 0].Pos.Y - DG.Field.Y - DG.Field.Height);

                        float Target = -GauD.GetMaximum(i * DG.PointSpacing, (i + 1) * DG.PointSpacing) / InversedMagnitude * 400;
                        DG.Points[i, 0].Vel.Y += ((Target + DG.Field.Y + DG.Field.Height - 10) - DG.Points[i, 0].Pos.Y) / 3f;
                    }
                }
            }
            if (VisSetting == Visualizations.grid)
            {
                DG.Update();
                for (int i = 1; i < DG.Points.GetLength(0) - 1; i++)
                    DG.Points[i, 0].Vel.Y += ((DG.Field.Y + DG.Field.Height - 30) - DG.Points[i, 0].Pos.Y) / 2.5f;
            }

            try
            {
                if (Config.Data.AutoVolume)
                    SongManager.Channel32.Volume = (1 - Values.OutputVolume) * Values.TargetVolume * Values.VolumeMultiplier; // Null pointer exception? 13.02.18 13:36 / 27.02.18 01:35
                else
                    SongManager.Channel32.Volume = 0.75f * Values.TargetVolume * Values.VolumeMultiplier; // Null pointer exception? 09.04.23 17:55
            }
            catch { }

            UpdateRectangles();

            WasFocusedLastFrame = gameWindowForm.Focused;

            //Debug.WriteLine("Update: " + (Stopwatch.GetTimestamp() - CurrentDebugTime).ToString());
            //base.Update(gameTime);
            //Debug.WriteLine("Update + base: " + (Stopwatch.GetTimestamp() - CurrentDebugTime).ToString());
        }
        public void CheckForRequestedSongs()
        {
            if (lastSongRequestCheck < Values.Timer - 15)
            {
                if (SongCheckThread != null)
                    SongCheckThread.Wait();

                SongCheckThread = Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.Name = "Check For Requested Songs Thread";
                    bool Worked = false;
                    while (!Worked && lastSongRequestCheck < Values.Timer - 5)
                    {
                        try
                        {
                            Thread.Sleep(500);
                            RequestedSong.Default.Reload();
                            if (RequestedSong.Default.RequestedSongString != "")
                            {
                                lastSongRequestCheck = (int)Values.Timer;
                                SongManager.PlayNewSong(RequestedSong.Default.RequestedSongString);
                                RequestedSong.Default.RequestedSongString = "";
                                RequestedSong.Default.Save();
                            }
                            Worked = true;
                        }
                        catch { }
                    }
                });
            }
        }
        void ComputeControls()
        {
            // Mouse Controls
            if (Control.WasLMBJustPressed() && gameWindowForm.Focused &&
                Control.GetMouseRect().Intersects(Values.WindowRect) ||
                !WasFocusedLastFrame && gameWindowForm.Focused &&
                Control.GetMouseRect().Intersects(Values.WindowRect))
            {
                CreateGlobalKeyHooks();
                MouseClickedPos.X = Control.CurMS.X;
                MouseClickedPos.Y = Control.CurMS.Y;

                if (Values.GetWindow(gameWindowForm.Handle, 2) != Shadow.Handle)
                {
                    Shadow.BringToFront();
                    gameWindowForm.BringToFront();
                }

                if (Control.GetMouseRect().Intersects(DurationBarHitbox))
                {
                    selectedControl = SelectedControl.DurationBar;
                    SkipStartPosition = SongManager.Channel32.Position;
                }
                else if (Control.GetMouseRect().Intersects(VolumeBarHitbox))
                    selectedControl = SelectedControl.VolumeSlider;
                else if (Control.GetMouseRect().Intersects(UpvoteButtonHitbox))
                    selectedControl = SelectedControl.UpvoteButton;
                else if (Control.GetMouseRect().Intersects(PlayPauseButtonHitbox))
                    selectedControl = SelectedControl.PlayPauseButton;
                else if (Control.GetMouseRect().Intersects(CloseButton))
                    selectedControl = SelectedControl.CloseButton;
                else if (Control.GetMouseRect().Intersects(OptionsButton))
                    selectedControl = SelectedControl.OptionsButton;
                else
                    selectedControl = SelectedControl.DragWindow;
            }
            if (Control.WasLMBJustPressed() && Control.GetMouseRect().Intersects(Values.WindowRect))
                wasClickedOn = true;
            if (Control.WasLMBJustReleased())
            {
                if (selectedControl == SelectedControl.DurationBar)
                {
                    SongTimeSkipped = SongManager.Channel32.Position - SkipStartPosition;
                    SongManager.output.Play();
                    UpdateDiscordRPC();
                }
                selectedControl = SelectedControl.None;
            }
            if (Control.CurMS.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                if (selectedControl == SelectedControl.DragWindow)
                    ForceBackgroundRedraw();
                selectedControl = SelectedControl.None;
            }

            switch (selectedControl)
            {
                case SelectedControl.VolumeSlider:
                    float value = (Control.GetMouseVector().X - VolumeBar.X) / (VolumeBar.Width / MaxVolume);
                    if (value < 0) value = 0;
                    if (value > MaxVolume) value = MaxVolume;
                    Values.TargetVolume = value;
                    break;

                case SelectedControl.PlayPauseButton:
                    if (Control.WasLMBJustPressed() || !WasFocusedLastFrame && gameWindowForm.Focused)
                        SongManager.PlayPause();
                    break;

                case SelectedControl.CloseButton:
                    if (Control.WasLMBJustPressed() || !WasFocusedLastFrame)
                    {
                        if (CloseConfirmationThread == null || CloseConfirmationThread.Status == TaskStatus.RanToCompletion)
                        {
                            CloseConfirmationThread = Task.Factory.StartNew(() =>
                            {
                                if (MessageBox.Show("Do you really want to close me? :<", "Quit!?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    Program.closing = true;
                                    gameWindowForm.InvokeIfRequired(gameWindowForm.Close);
                                    DiscordRPCWrapper.Shutdown();
                                }
                            });
                        }
                    }
                    break;

                case SelectedControl.OptionsButton:
                    if (Control.WasLMBJustPressed() || !WasFocusedLastFrame && gameWindowForm.Focused)
                        ShowOptions();
                    break;

                case SelectedControl.UpvoteButton:
                    if (Control.WasLMBJustPressed() || !WasFocusedLastFrame && gameWindowForm.Focused)
                        SongManager.IsCurrentSongUpvoted = !SongManager.IsCurrentSongUpvoted;
                    break;

                case SelectedControl.DurationBar:
                    SongManager.Channel32.Position =
                           (long)(((Control.GetMouseVector().X - DurationBar.X) / DurationBar.Width) *
                           SongManager.Channel32.TotalTime.TotalSeconds * SongManager.Channel32.WaveFormat.AverageBytesPerSecond);

                    if (Control.CurMS.X == Control.LastMS.X)
                        SongManager.output.Pause();
                    else
                        SongManager.output.Play();
                    break;

                case SelectedControl.DragWindow:
                    oldPos.X = Config.Data.WindowPos.X;
                    oldPos.Y = Config.Data.WindowPos.Y;

                    newPos.X = gameWindowForm.Location.X + Control.CurMS.X - MouseClickedPos.X;
                    newPos.Y = gameWindowForm.Location.Y + Control.CurMS.Y - MouseClickedPos.Y;

                    if (newPos.X % 50 == 0)
                        GetHashCode();
                    
                    Config.Data.WindowPos = newPos;
                    KeepWindowInScreen();
                    ForceBackgroundRedraw();

                    if (VisSetting == Visualizations.grid)
                    {
                        TempVector.X = oldPos.X - Config.Data.WindowPos.X;
                        TempVector.Y = oldPos.Y - Config.Data.WindowPos.Y;
                        DG.ApplyForceGlobally(TempVector);
                    }
                    break;
            }
            gameWindowForm.Location = Config.Data.WindowPos;

            if (!wasClickedOn)
                return;

            // Pause [Space]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Space))
                SongManager.PlayPause();

            // Set Location to (0, 0) [0]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D0))
                Config.Data.WindowPos = new System.Drawing.Point(0, 0);

            // Open OptionsMenu [O / F1]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.O) ||
                Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F1))
                ShowOptions();

            // Swap Visualisations [V]
                if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                VisSetting++;
                if ((int)VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                    VisSetting = 0;

                if (VisSetting == Visualizations.dynamicline)
                    VisSetting = Visualizations.fft;
            }

            // Swap Backgrounds [B]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                BgModes++;
                if ((int)BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                    BgModes = 0;
                if (BgModes == BackGroundModes.None)
                    Shadow.Opacity = 0;
                else if (BgModes == BackGroundModes.None + 1)
                {
                    Shadow.Dispose();
                    Shadow = new DropShadow(gameWindowForm, true);
                    Shadow.Show();
                    Shadow.UpdateSizeLocation();
                    gameWindowForm.BringToFront();
                }
                ForceBackgroundRedraw();
            }

            // New Color [C]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.C))
                ShowColorDialog();

            // Copy URL the Clipboard [U]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            {
                Values.StartSTATask(() =>
                {
                    try
                    {
                        Clipboard.SetText(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName).GetYoutubeVideoURL());
                        ShowSecondRowMessage("Copied  URL  to  clipboard!", 1);
                    }
                    catch (Exception e) { MessageBox.Show("Can't find that song.\n\nException: " + e.ToString()); }
                });
            }

            // Restart [R]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.R))
            {
                Values.StartSTATask(() =>
                {
                    try
                    {
                        if (MessageBox.Show("Do you really want to restart?", "Restart?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Program.closing = true;
                            gameWindowForm.InvokeIfRequired(gameWindowForm.Close);
                            DiscordRPCWrapper.Shutdown();
                            Application.Exit();
                            Program.Restart();
                        }
                    }
                    catch (Exception e) { MessageBox.Show("Can't restart.\n\nException: " + e.ToString()); }
                });
            }

            // Show Console [K]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.K))
            {
                Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                Values.SetForegroundWindow(Values.GetConsoleWindow());
            }

            // Toggle Anti-Alising [A]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
                Config.Data.AntiAliasing = !Config.Data.AntiAliasing;

            // Toggle Preload [P]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            {
                Preload = !Preload;
                ShowSecondRowMessage("Preload was set to " + Preload + " \nThis setting will be applied when the next song starts", 1);
            }

            // Upvote/Like Current Song [L]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.L))
                SongManager.IsCurrentSongUpvoted = !SongManager.IsCurrentSongUpvoted;

            // Higher / Lower Volume [Up/Down]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                Values.TargetVolume += 0.005f;
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                Values.TargetVolume -= 0.005f;
            if (Values.TargetVolume > MaxVolume)
                Values.TargetVolume = MaxVolume;
            if (Values.TargetVolume < 0)
                Values.TargetVolume = 0;

            // Next / Previous Song [Left/Right]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Left))
                SongManager.GetPreviousSong();
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Right))
                SongManager.GetNextSong(false, true);

            // Close [Esc]
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && base.IsActive)
            {
                Task.Factory.StartNew(() =>
                {
                    if (MessageBox.Show("Do you really want to close me? :<", "Quit!?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Program.closing = true;
                        gameWindowForm.InvokeIfRequired(gameWindowForm.Close);
                        DiscordRPCWrapper.Shutdown();
                        Application.Exit();
                    }
                });
            }

            // Show Music File in Explorer [E]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                if (!File.Exists(SongManager.currentlyPlayingSongPath))
                    return;
                else
                    Process.Start("explorer.exe", "/select, \"" + SongManager.currentlyPlayingSongPath + "\"");
            }

            // Show Music File in Browser [I]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.I))
            {
                Values.StartSTATask(() =>
                {
                    try
                    {
                        // Get fitting youtube video
                        string url = string.Format("https://www.youtube.com/results?search_query=" + Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName));
                        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                        req.KeepAlive = false;
                        WebResponse W = req.GetResponse();
                        string ResultURL;
                        using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                        {
                            string html = sr.ReadToEnd();
                            int index = html.IndexOf("href=\"/watch?");
                            string startcuthtml = html.Remove(0, index + 6);
                            index = startcuthtml.IndexOf('"');
                            string cuthtml = startcuthtml.Remove(index, startcuthtml.Length - index);
                            ResultURL = "https://www.youtube.com" + cuthtml;
                        }

                        Process.Start(ResultURL);
                    }
                    catch (Exception e) { MessageBox.Show("Can't find that song.\n\nException: " + e.ToString()); }
                });
            }

            // Show Statistics [S]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
                ShowStatistics();
        }
        public void ShowColorDialog()
        {
            Task.Factory.StartNew(() =>
            {
                if (LeDialog == null)
                {
                    LeDialog = new ColorDialog();
                    LeDialog.AllowFullOpen = true;
                    LeDialog.AnyColor = true;
                    LeDialog.Color = System.Drawing.Color.FromArgb(primaryColor.A, primaryColor.R, primaryColor.G, primaryColor.B);

                    Color[] Background = new Color[Assets.bg.Bounds.Width * Assets.bg.Bounds.Height];
                    Vector3 AvgColor = new Vector3();
                    Assets.bg.GetData(Background);

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.X += Background[i].R;
                    AvgColor.X /= Background.Length;

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.Y += Background[i].G;
                    AvgColor.Y /= Background.Length;

                    for (int i = 0; i < Background.Length; i++)
                        AvgColor.Z += Background[i].B;
                    AvgColor.Z /= Background.Length;

                    System.Drawing.Color AvgC = System.Drawing.Color.FromArgb(255, (int)AvgColor.Z, (int)AvgColor.Y, (int)AvgColor.X);
                    System.Drawing.Color DefC = System.Drawing.Color.FromArgb(255, 255, 75, 25);
                    System.Drawing.Color SysC = System.Drawing.Color.FromArgb(255, Assets.SystemDefaultColor.B, Assets.SystemDefaultColor.G, Assets.SystemDefaultColor.R);

                    LeDialog.CustomColors = new int[]{
                        SysC.ToArgb(), SysC.ToArgb(), SysC.ToArgb(), SysC.ToArgb(), AvgC.ToArgb(), AvgC.ToArgb(), AvgC.ToArgb(), AvgC.ToArgb(),
                        DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb(), DefC.ToArgb()};
                }
                if (!ShowingColorDialog)
                {
                    ShowingColorDialog = true;
                    DialogResult DR = LeDialog.ShowDialog();
                    if (DR == DialogResult.OK)
                    {
                        primaryColor = Color.FromNonPremultiplied(LeDialog.Color.R, LeDialog.Color.G, LeDialog.Color.B, LeDialog.Color.A);
                        secondaryColor = Color.Lerp(primaryColor, Color.White, 0.4f);
                    }
                    ShowingColorDialog = false;
                }
            });
        }
        public void ResetMusicSourcePath()
        {
            DialogResult DR = MessageBox.Show("Are you sure you want to reset the music source path?", "Source Path Reset", MessageBoxButtons.YesNo);

            if (DR != DialogResult.Yes)
                return;

            Config.Data.MusicPath = "";
            Config.Save();
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C ping 127.0.0.1 -n 2 && cls && \"" + Application.ExecutablePath + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }
        public void UpdateGD()
        {
            //CurrentDebugTime = Stopwatch.GetTimestamp();
            if (SongVisualization.FFToutput != null)
            {
                //Debug.WriteLine("GD Update 0 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
                //CurrentDebugTime = Stopwatch.GetTimestamp();
                float ReadLength = SongVisualization.FFToutput.Length / 3f;
                for (int i = 70; i < Values.WindowSize.X; i++)
                {
                    double lastindex = Math.Pow(ReadLength, (i - 1) / (double)Values.WindowSize.X);
                    double index = Math.Pow(ReadLength, i / (double)Values.WindowSize.X);
                    values[i - 70] = SongVisualization.GetMaxHeight(SongVisualization.FFToutput, (int)lastindex, (int)index) * Values.VolumeMultiplier * 2;
                }
                //Debug.WriteLine("GD Update 1 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
                //CurrentDebugTime = Stopwatch.GetTimestamp();
                GauD.Update(values);
                if (Config.Data.OldSmooth)
                    GauD.Smoothen();
                else
                    GauD.NewSmoothen(Config.Data.Smoothness);
                //Debug.WriteLine("New Smooth " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
            }
        }
        public void UpdateRectangles()
        {
            TargetVolumeBar.X = Values.WindowSize.X - 100;
            TargetVolumeBar.Y = 24 + 0;
            TargetVolumeBar.Width = (int)(75 * Values.TargetVolume / MaxVolume);
            TargetVolumeBar.Height = 8;

            if (SongManager.Channel32 != null)
            {
                ActualVolumeBar.X = Values.WindowSize.X - 25 - 75;
                ActualVolumeBar.Y = 24;
                ActualVolumeBar.Width = (int)(75 * SongManager.Channel32.Volume / Values.VolumeMultiplier / MaxVolume);
                if (ActualVolumeBar.Width > 75)
                    ActualVolumeBar.Width = 75;
                ActualVolumeBar.Height = 8;
            }
        }
        public void KeepWindowInScreen()
        {
            TempRect.X = Config.Data.WindowPos.X;
            TempRect.Y = Config.Data.WindowPos.Y;
            TempRect.Width = gameWindowForm.Bounds.Width;
            TempRect.Height = gameWindowForm.Bounds.Height;
            Config.Data.WindowPos = KeepWindowInScreen(TempRect);
        }
        public System.Drawing.Point KeepWindowInScreen(Rectangle WindowBounds)
        {
            Rectangle[] ScreenBoxes = new Rectangle[Screen.AllScreens.Length];

            for (int i = 0; i < ScreenBoxes.Length; i++)
                ScreenBoxes[i] = new Rectangle(Screen.AllScreens[i].WorkingArea.X, Screen.AllScreens[i].WorkingArea.Y,
                    Screen.AllScreens[i].WorkingArea.Width, Screen.AllScreens[i].WorkingArea.Height - 56);

            WindowPoints[0] = new Point(WindowBounds.X, WindowBounds.Y);
            WindowPoints[1] = new Point(WindowBounds.X + WindowBounds.Width, WindowBounds.Y);
            WindowPoints[2] = new Point(WindowBounds.X, WindowBounds.Y + WindowBounds.Height);
            WindowPoints[3] = new Point(WindowBounds.X + WindowBounds.Width, WindowBounds.Y + WindowBounds.Height);

            Screen Main = Screen.FromRectangle(new System.Drawing.Rectangle(WindowBounds.X, WindowBounds.Y, WindowBounds.Width, WindowBounds.Height));
            if (Main == null)
                Main = Screen.PrimaryScreen;

            for (int i = 0; i < WindowPoints.Length; i++)
                if (!RectanglesContainPoint(WindowPoints[i], ScreenBoxes))
                {
                    Diff = PointRectDiff(WindowPoints[i], new Rectangle(Main.WorkingArea.X, Main.WorkingArea.Y, Main.WorkingArea.Width, Main.WorkingArea.Height));

                    if (Diff != new Point(0, 0))
                    {
                        WindowBounds = new Rectangle(WindowBounds.X + Diff.X, WindowBounds.Y + Diff.Y, WindowBounds.Width, WindowBounds.Height);

                        WindowPoints[0] = new Point(WindowBounds.X, WindowBounds.Y);
                        WindowPoints[1] = new Point(WindowBounds.X + WindowBounds.Width, WindowBounds.Y);
                        WindowPoints[2] = new Point(WindowBounds.X, WindowBounds.Y + WindowBounds.Height);
                        WindowPoints[3] = new Point(WindowBounds.X + WindowBounds.Width, WindowBounds.Y + WindowBounds.Height);
                    }
                }

            return new System.Drawing.Point(WindowBounds.X, WindowBounds.Y);
        }
        static bool RectanglesContainPoint(Point P, Rectangle[] R)
        {
            for (int i = 0; i < R.Length; i++)
                if (R[i].Contains(P))
                    return true;
            return false;
        }
        static Point PointRectDiff(Point P, Rectangle R)
        {
            if (P.X > R.X && P.X < R.X + R.Width &&
                P.Y > R.Y && P.Y < R.Y + R.Height)
                return new Point(0, 0);
            else
            {
                Point r = new Point(0, 0);
                if (P.X < R.X)
                    r.X = R.X - P.X;
                if (P.X > R.X + R.Width)
                    r.X = R.X + R.Width - P.X;
                if (P.Y < R.Y)
                    r.Y = R.Y - P.Y;
                if (P.Y > R.Y + R.Height)
                    r.Y = R.Y + R.Height - P.Y;
                return r;
            }
        }
        public void ShowSecondRowMessage(string Message, float StartingAlpha)
        {
            SecondRowMessageAlpha = StartingAlpha;
            SecondRowMessageText = Message;
        }
        public void ShowStatistics()
        {
            if (statistics == null || statistics.IsClosed || statistics.IsDisposed || !statistics.Created)
            {
                statistics = new Statistics(this);
                Values.StartSTATask(() => { statistics.ShowDialog(); });
            }
            else
                statistics.InvokeIfRequired(() => { statistics.RestoreFromMinimzied(); Values.SetForegroundWindow(statistics.Handle); });
        }
        public void ShowOptions()
        {
            if (optionsMenu == null || optionsMenu.IsClosed || optionsMenu.IsDisposed || !optionsMenu.HasBeenShown)
            {
                optionsMenu = new OptionsMenu(this);
                Values.StartSTATask(() => { optionsMenu.ShowDialog(); });
            }
            else
                optionsMenu.InvokeIfRequired(() => { optionsMenu.RestoreFromMinimzied(); Values.SetForegroundWindow(optionsMenu.Handle); });
        }
        public void UpdateDiscordRPC()
        {
            // Old Arguments
            bool IsPlaying = (SongManager.output.PlaybackState == PlaybackState.Playing);
            bool ElapsedTime = true;

            string SongName = Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongPath);
            string[] SongNameSplit = SongName.Split('-');

            DateTime startTime, endTime;
            string smolimagekey = "", smolimagetext = "", bigimagekey = "iconv2";
            if (IsPlaying)
            {
                startTime = DateTime.Now.AddSeconds(-(SongManager.Channel32.Position / (double)SongManager.Channel32.Length) * SongManager.Channel32.TotalTime.TotalSeconds);
                endTime = DateTime.Now.AddSeconds((1 - SongManager.Channel32.Position / (double)SongManager.Channel32.Length) * SongManager.Channel32.TotalTime.TotalSeconds);

                smolimagekey = "playv2";
                smolimagetext = "Playing";
            }
            else
            {
                startTime = DateTime.FromBinary(0);
                endTime = DateTime.FromBinary(0);

                smolimagekey = "pausev3";
                smolimagetext = "Paused";
            }

            string details, state, time;
            if (IsPlaying)
                time = " (" + Values.AsTime(SongManager.Channel32.TotalTime.TotalSeconds) + ")";
            else
                time = " (" + Values.AsTime(SongManager.Channel32.Position / (double)SongManager.Channel32.Length * SongManager.Channel32.TotalTime.TotalSeconds) + " / " + Values.AsTime(SongManager.Channel32.TotalTime.TotalSeconds) + ")";
            if (SongName.Length < 20)
            {
                details = SongName;
                state = "";
            }
            else if (SongNameSplit.Length == 2)
            {
                details = SongNameSplit[0].Trim(' ');
                state = SongNameSplit[1].Trim(' ');
            }
            else if (SongNameSplit.Length > 2)
            {
                details = SongNameSplit.Reverse().Skip(1).Reverse().Aggregate((i, j) => i + "-" + j);
                state = SongNameSplit.Last();
            }
            else
            {
                details = SongName;
                state = "";
            }
#if DEBUG
            details = details.Insert(0, "[DEBUG-VERSION] ");
            bigimagekey = "debugiconv1";
#endif
            if (details.Length < state.Length)
            {
                details += time;
            }
            else
            {
                state += time;
                state = state.TrimStart(' ');
            }
            
            DiscordRPCWrapper.UpdatePresence(details, state, startTime, endTime, bigimagekey, "https://github.com/jnccd/music-player-dxmg-port", smolimagekey, smolimagetext, ElapsedTime);
        }
        public void UpdateShadowRects()
        {
            DurationBarShadow = new Rectangle(DurationBar.X + Config.Data.ShadowDistance, DurationBar.Y + Config.Data.ShadowDistance, DurationBar.Width, DurationBar.Height);
            VolumeIconShadow = new Rectangle(VolumeIcon.X + Config.Data.ShadowDistance, VolumeIcon.Y + Config.Data.ShadowDistance, VolumeIcon.Width, VolumeIcon.Height);
            VolumeBarShadow = new Rectangle(VolumeBar.X + Config.Data.ShadowDistance, VolumeBar.Y + Config.Data.ShadowDistance, VolumeBar.Width, VolumeBar.Height);
            PlayPauseButtonShadow = new Rectangle(PlayPauseButton.X + Config.Data.ShadowDistance, PlayPauseButton.Y + Config.Data.ShadowDistance, PlayPauseButton.Width, PlayPauseButton.Height);
            UpvoteShadow = new Rectangle(Upvote.X + Config.Data.ShadowDistance, Upvote.Y + Config.Data.ShadowDistance, Upvote.Width, Upvote.Height);
            UpvoteButtonShadow = new Rectangle(UpvoteButton.X + Config.Data.ShadowDistance, UpvoteButton.Y + Config.Data.ShadowDistance, UpvoteButton.Width, UpvoteButton.Height);
            CloseButtonShadow = new Rectangle(CloseButton.X + Config.Data.ShadowDistance, CloseButton.Y + Config.Data.ShadowDistance, CloseButton.Width, CloseButton.Height);
            OptionsButtonShadow = new Rectangle(OptionsButton.X + Config.Data.ShadowDistance, OptionsButton.Y + Config.Data.ShadowDistance, OptionsButton.Width, OptionsButton.Height);
        }
        // Keyhook
        public void CreateGlobalKeyHooks()
        {
            if (_rawinput == null)
            {
                _rawinput = new RawInput(gameWindowForm.Handle, false);
                _rawinput.AddMessageFilter();
                _rawinput.KeyPressed += OnRawKeyPressed;
            }
        }
        public void KeyHook_KeyDown(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
            if (Values.Timer > SongManager.SongChangedTickTime + 30)
            {
                var k = key;

                // Key Events
                if (k == Keys.MediaPlayPause)
                    SongManager.PlayPause();

                if (k == Keys.MediaNextTrack)
                    SongManager.GetNextSong(false, true);

                if (k == Keys.MediaPreviousTrack)
                    SongManager.GetPreviousSong();

                if (k == Keys.MediaStop)
                    SongManager.IsCurrentSongUpvoted = !SongManager.IsCurrentSongUpvoted;
            }
        }
        private void OnRawKeyPressed(object sender, RawInputEventArg e)
        {
            //Debug.WriteLine($"Event! {(Keys)e.KeyPressEvent.VKey}");
            if (e.KeyPressEvent.KeyPressState == "MAKE")
                KeyHook_KeyDown((Keys)e.KeyPressEvent.VKey, false, false, false);
        }
        public void DisposeGlobalKeyHooks()
        {
            _rawinput.KeyPressed -= OnRawKeyPressed;
        }

        // Draw
        protected override void Draw(GameTime gameTime)
        {
            //CurrentDebugTime = Stopwatch.GetTimestamp();

            // RenderTargets
            #region RT
            // Song Title
            if (ForcedTitleRedraw || TitleTarget == null || TitleTarget.IsContentLost || TitleTarget.IsDisposed)
            {
                Title = SongManager.currentlyPlayingSongName;
                while (Title.EndsWith(".mp3"))
                    Title = Title.Remove(Title.Length - 4);

                char[] arr = Title.ToCharArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] >= 32 && arr[i] <= 216
                            || arr[i] >= 8192 && arr[i] <= 10239
                            || arr[i] >= 12288 && arr[i] <= 12352
                            || arr[i] >= 65280 && arr[i] <= 65519)
                        arr[i] = arr[i];
                    else
                        arr[i] = (char)10060;
                }
                Title = new string(arr);

                int length = 0;
                if (TitleTarget == null)
                {
                    TitleTarget = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X - 166, (int)Assets.Title.MeasureString("III()()()III").Y);
                    length = (int)Assets.Title.MeasureString(Title).X;
                    X1 = startX;
                    X2 = X1 + length + abstand;
                }

                if (length == 0)
                    length = (int)Assets.Title.MeasureString(Title).X;
                if (length > TitleTarget.Bounds.Width)
                {
                    LongTitle = true;
                    X1 -= speed;
                    X2 -= speed;

                    if (X1 < -length)
                        X1 = X2 + length + abstand;
                    if (X2 < -length)
                        X2 = X1 + length + abstand;

                    GraphicsDevice.SetRenderTarget(TitleTarget);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                    
                    try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(X1 + Config.Data.ShadowDistance, Config.Data.ShadowDistance), Color.Black * 0.6f); } catch { }
                    try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(X1, 0), backgroundColor); } catch { }

                    try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(X2 + Config.Data.ShadowDistance, Config.Data.ShadowDistance), Color.Black * 0.6f); } catch { }
                    try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(X2, 0), backgroundColor); } catch { }
                }
                else
                {
                    LongTitle = false;

                    GraphicsDevice.SetRenderTarget(TitleTarget);
                    GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);

                    try { spriteBatch.DrawString(Assets.Title, Title, new Vector2(Config.Data.ShadowDistance), Color.Black * 0.6f); } catch { }
                    try { spriteBatch.DrawString(Assets.Title, Title, Vector2.Zero, Color.White); } catch { }
                }

                ForcedTitleRedraw = false;

                spriteBatch.End();
            }

            // FFT Diagram
            if (VisSetting == Visualizations.fft && SongManager.output != null && SongManager.output.PlaybackState == PlaybackState.Playing || SongManager.output != null && GauD.WasRenderTargetContentLost())
            {
                //CurrentDebugTime2 = Stopwatch.GetTimestamp();
                //GauD.DrawToRenderTarget3DAcc(spriteBatch, GraphicsDevice);
                //Debug.WriteLine("Draw GauD 3DACC " + (Stopwatch.GetTimestamp() - CurrentDebugTime2));

                //CurrentDebugTime2 = Stopwatch.GetTimestamp();
                GauD.DrawToRenderTarget(spriteBatch, GraphicsDevice);
                //Debug.WriteLine("Draw GauD " + (Stopwatch.GetTimestamp() - CurrentDebugTime2));
            }

            // Background
            ForcedBackgroundRedraw = true;
            if (ForcedBackgroundRedraw || BackgroundTarget == null || BackgroundTarget.IsContentLost || BackgroundTarget.IsDisposed || ForcedCoverBackgroundRedraw)
            {
                if (oldPos.X == 0 && oldPos.Y == 0)
                    oldPos = newPos;

                if (BgModes == BackGroundModes.Blur || BgModes == BackGroundModes.BlurVignette)
                {
                    BeginBlur();
                    spriteBatch.Begin();
                    // Blurred Background
                    foreach (Screen S in Screen.AllScreens)
                    {
                        TempRect.X = S.Bounds.X - gameWindowForm.Location.X + 50;
                        TempRect.Y = S.Bounds.Y - gameWindowForm.Location.Y + 50;
                        TempRect.Width = S.Bounds.Width;
                        TempRect.Height = S.Bounds.Height;
                        spriteBatch.Draw(Assets.bg, TempRect, Color.White);
                    }
                    spriteBatch.End();
                    EndBlur();
                }
                if (BgModes == BackGroundModes.BlurTeint)
                {
                    BeginBlur();
                    spriteBatch.Begin();
                    // Blurred Background
                    foreach (Screen S in Screen.AllScreens)
                    {
                        TempRect.X = S.Bounds.X - gameWindowForm.Location.X + 50;
                        TempRect.Y = S.Bounds.Y - gameWindowForm.Location.Y + 50;
                        TempRect.Width = S.Bounds.Width;
                        TempRect.Height = S.Bounds.Height;
                        spriteBatch.Draw(Assets.bg, TempRect, Color.White);
                    }
                    TempRect.X = 0;
                    TempRect.Y = 0;
                    TempRect.Width = Values.WindowRect.Width + 100;
                    TempRect.Height = Values.WindowRect.Height + 100;
                    spriteBatch.Draw(Assets.White, TempRect, Color.FromNonPremultiplied(0, 0, 0, 75));
                    spriteBatch.End();
                    EndBlur();
                }
                
                if (BackgroundTarget == null)
                    BackgroundTarget = new RenderTarget2D(GraphicsDevice, Values.WindowSize.X, Values.WindowSize.Y);
                GraphicsDevice.SetRenderTarget(BackgroundTarget);
                GraphicsDevice.Clear(Color.Transparent);

                if (BgModes == BackGroundModes.Blur || BgModes == BackGroundModes.BlurTeint || BgModes == BackGroundModes.BlurVignette)
                    DrawBlurredTex();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);

                if (BgModes == BackGroundModes.None)
                    foreach (Screen S in Screen.AllScreens)
                    {
                        TempRect.X = S.Bounds.X - gameWindowForm.Location.X;
                        TempRect.Y = S.Bounds.Y - gameWindowForm.Location.Y;
                        TempRect.Width = S.Bounds.Width;
                        TempRect.Height = S.Bounds.Height;
                        spriteBatch.Draw(Assets.bg, TempRect, Color.White);
                    }
                else if (BgModes == BackGroundModes.coverPic)
                {
                    if (Assets.CoverPicture == null || ForcedCoverBackgroundRedraw)
                    {
                        try
                        {
                            string path = SongManager.currentlyPlayingSongPath;
                            TagLib.File file = TagLib.File.Create(path);
                            TagLib.IPicture pic = file.Tag.Pictures[0];
                            MemoryStream ms = new MemoryStream(pic.Data.Data);
                            if (ms != null && ms.Length > 4096)
                            {
                                System.Drawing.Image currentImage = System.Drawing.Image.FromStream(ms);
                                path = Values.CurrentExecutablePath + "\\Downloads\\Thumbnail.png";
                                Directory.CreateDirectory(Path.GetDirectoryName(path));
                                using (var stream = File.Create(path)) { }
                                currentImage.Save(path);
                                Assets.CoverPicture = Texture2D.FromStream(Program.game.GraphicsDevice, new FileStream(path, FileMode.Open));
                            }
                            ms.Close();
                        }
                        catch { }
                        ForcedCoverBackgroundRedraw = false;
                    }

                    if (Assets.CoverPicture != null)
                        spriteBatch.Draw(Assets.CoverPicture, Values.WindowRect, Color.White);
                }

                // Borders
                if (BgModes != BackGroundModes.None)
                {
                    TempRect.X = Values.WindowSize.X - 1;
                    TempRect.Y = 0;
                    TempRect.Width = 1;
                    TempRect.Height = Values.WindowSize.Y;
                    spriteBatch.Draw(Assets.White, TempRect, Color.Gray * 0.25f);
                    TempRect.X = 0;
                    spriteBatch.Draw(Assets.White, TempRect, Color.Gray * 0.25f);
                    TempRect.Width = Values.WindowSize.X;
                    TempRect.Height = 1;
                    spriteBatch.Draw(Assets.White, TempRect, Color.Gray * 0.25f);
                    TempRect.Y = Values.WindowSize.Y - 1;
                    spriteBatch.Draw(Assets.White, TempRect, Color.Gray * 0.25f);
                }

                spriteBatch.End();
                ForcedBackgroundRedraw = false;
                ForcedCoverBackgroundRedraw = false;
            }
            #endregion

            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);

            lock (GauD)
            {
                lock (BackgroundTarget)
                {
                    if (BgModes == BackGroundModes.BlurVignette)
                    {
                        spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, Assets.Vignette);
                        spriteBatch.Draw(BackgroundTarget, Values.WindowRect, Color.White);
                        spriteBatch.End();
                        spriteBatch.Begin();
                    }
                    else
                    {
                        spriteBatch.Begin();
                        spriteBatch.Draw(BackgroundTarget, Values.WindowRect, Color.White);
                    }
                    
                    #region Second Row HUD Shadows
                    if (UpvoteSavedAlpha > 0)
                    {
                        TempVector.X = Upvote.X + Upvote.Width + 3 + Config.Data.ShadowDistance;
                        TempVector.Y = Upvote.Y + Upvote.Height / 2 - 8 + Config.Data.ShadowDistance;
                        spriteBatch.Draw(Assets.Upvote, UpvoteShadow, Color.Black * 0.6f * UpvoteSavedAlpha);
                        //spriteBatch.DrawString(Assets.Font, "Upvote saved! (" + Assets.LastUpvotedSongStreak.ToString() + " points)", new Vector2(Upvote.X + Upvote.Width + 8, Upvote.Y + Upvote.Height / 2 - 3), Color.Black * 0.6f * UpvoteSavedAlpha);
                        spriteBatch.DrawString(Assets.Font, "Upvote saved!", TempVector, Color.Black * 0.6f * UpvoteSavedAlpha);
                    }
                    else if (SecondRowMessageAlpha > 0)
                    {
                        TempVector.X = 24 + Config.Data.ShadowDistance;
                        TempVector.Y = 45 + Config.Data.ShadowDistance;
                        if (SecondRowMessageAlpha > 1)
                            spriteBatch.DrawString(Assets.Font, SecondRowMessageText, TempVector, Color.Black * 0.6f);
                        else
                            spriteBatch.DrawString(Assets.Font, SecondRowMessageText, TempVector, Color.Black * 0.6f * SecondRowMessageAlpha);
                    }
                    #endregion

                    // Visualizations
                    #region Line graph
                    // Line Graph
                    if (VisSetting == Visualizations.line && SongManager.Channel32 != null)
                    {
                        float Height = Values.WindowSize.Y / 1.96f;
                        int StepLength = SongVisualization.WaveBuffer.Length / 512;

                        // Shadow
                        for (int i = 1; i < 512; i++)
                        {
                            SongVisualization.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (SongVisualization.WaveBuffer.Length / StepLength) + Config.Data.ShadowDistance,
                                            Height + (int)(SongVisualization.WaveBuffer[(i - 1) * StepLength] * 100) + Config.Data.ShadowDistance),

                                            new Vector2(i * Values.WindowSize.X / (SongVisualization.WaveBuffer.Length / StepLength) + Config.Data.ShadowDistance,
                                            Height + (int)(SongVisualization.WaveBuffer[i * StepLength] * 100) + Config.Data.ShadowDistance),

                                            2, Color.Black * 0.6f, spriteBatch);
                        }

                        for (int i = 1; i < 512; i++)
                        {
                            SongVisualization.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (SongVisualization.WaveBuffer.Length / StepLength),
                                            Height + (int)(SongVisualization.WaveBuffer[(i - 1) * StepLength] * 100)),

                                            new Vector2(i * Values.WindowSize.X / (SongVisualization.WaveBuffer.Length / StepLength),
                                            Height + (int)(SongVisualization.WaveBuffer[i * StepLength] * 100)),

                                            2, Color.Lerp(primaryColor, secondaryColor, i / 512), spriteBatch);
                        }
                    }
                    #endregion
                    #region Dynamic Line graph
                    // Line Graph
                    if (VisSetting == Visualizations.dynamicline && SongManager.Channel32 != null)
                    {
                        float Height = Values.WindowSize.Y / 1.96f;
                        int StepLength = SongVisualization.WaveBuffer.Length / 512;
                        float MostUsedFrequency = Array.IndexOf(SongVisualization.RawFFToutput, SongVisualization.RawFFToutput.Max());
                        float MostUsedWaveLength = 10000;
                        if (MostUsedFrequency != 0)
                            MostUsedWaveLength = 1 / MostUsedFrequency;
                        float[] MostUsedFrequencyMultiplications = new float[100];
                        for (int i = 1; i <= 100; i++)
                            MostUsedFrequencyMultiplications[i - 1] = MostUsedFrequency * i;
                        //Debug.WriteLine((MostUsedFrequency / SongManager.Channel32.WaveFormat.SampleRate * Assets.RawFFToutput.Length) + " ::: " + MostUsedFrequency);

                        // Shadow
                        for (int i = 1; i < 512; i++)
                        {
                            SongVisualization.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (512) + Config.Data.ShadowDistance,
                                            Height + (int)(SongVisualization.WaveBuffer[(i - 1) * StepLength] * 100) + Config.Data.ShadowDistance),

                                            new Vector2(i * Values.WindowSize.X / (512) + Config.Data.ShadowDistance,
                                            Height + (int)(SongVisualization.WaveBuffer[i * StepLength] * 100) + Config.Data.ShadowDistance),

                                            2, Color.Black * 0.6f, spriteBatch);
                        }

                        for (int i = 1; i < 512; i++)
                        {
                            SongVisualization.DrawLine(new Vector2((i - 1) * Values.WindowSize.X / (512),
                                            Height + (int)(SongVisualization.WaveBuffer[(i - 1) * StepLength] * 100)),

                                            new Vector2(i * Values.WindowSize.X / (512),
                                            Height + (int)(SongVisualization.WaveBuffer[i * StepLength] * 100)),

                                            2, Color.Lerp(primaryColor, secondaryColor, i / 512), spriteBatch);
                        }
                    }
                    #endregion
                    #region FFT Graph
                    // FFT Graph
                    if (VisSetting == Visualizations.fft && SongManager.Channel32 != null && SongVisualization.FFToutput != null)
                        GauD.DrawRenderTarget(spriteBatch);
                    #endregion
                    #region Trumpet boy
                    // FFT Graph
                    if (VisSetting == Visualizations.trumpetboy && SongManager.Channel32 != null && SongVisualization.FFToutput != null)
                    {
                        float size = (float)Approximate.Sqrt(Values.OutputVolume * 100 * Values.TargetVolume);
                        
                        spriteBatch.Draw(Assets.White, new Rectangle(35 + Config.Data.ShadowDistance, 50 + Config.Data.ShadowDistance, Values.WindowSize.X - 70, Values.WindowSize.Y - 100), Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.TrumpetBoyBackground, new Rectangle(35, 50, Values.WindowSize.X - 70, Values.WindowSize.Y - 100), Color.White);

                        int x = 290;
                        int y = 100;
                        int width = (int)(208 * (Values.WindowSize.X - 70) / 1280 * 1.05f);
                        int height = (int)(450 * (Values.WindowSize.Y - 100) / 720 * 1.05f);
                        int yOrigin = (int)(60 * height / 450f);
                        spriteBatch.Draw(Assets.TrumpetBoy, new Rectangle(x, y, width, height), Color.White);
                        spriteBatch.Draw(Assets.TrumpetBoyTrumpet, new Rectangle((int)(x + width / 2f - width / 2f * size), 
                            (int)(y + yOrigin - yOrigin * size), (int)(width * size), (int)(height * size)), Color.White);
                    }
                    #endregion
                    #region Raw FFT Graph
                    if (VisSetting == Visualizations.rawfft && SongManager.Channel32 != null && SongVisualization.FFToutput != null)
                    {
                        GauD.DrawInputData(spriteBatch);
                    }
                    #endregion
                    #region FFT Bars
                    // FFT Bars
                    if (VisSetting == Visualizations.barchart)
                    {
                        GauD.DrawAsBars(spriteBatch);
                    }
                    #endregion
                    #region Grid
                    // Grid
                    if (VisSetting == Visualizations.grid)
                    {
                        DG.DrawShadow(spriteBatch);
                        DG.Draw(spriteBatch);
                    }
                    #endregion

                    // HUD
                    #region HUD
                    // Duration Bar
                    spriteBatch.Draw(Assets.White, DurationBarShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.White, DurationBar, backgroundColor);
                    if (SongManager.Channel32 != null)
                    {
                        lock (SongManager.Channel32)
                        {
                            float PlayPercetage = (SongManager.Channel32.Position / (float)SongManager.Channel32.WaveFormat.AverageBytesPerSecond /
                                ((float)SongManager.Channel32.TotalTime.TotalSeconds));
                            TempRect.X = DurationBar.X;
                            TempRect.Y = DurationBar.Y;
                            TempRect.Width = (int)(DurationBar.Width * PlayPercetage);
                            TempRect.Height = 3;
                            spriteBatch.Draw(Assets.White, TempRect, primaryColor);
                            if (SongVisualization.EntireSongWaveBuffer != null && Config.Data.Preload)
                            {
                                double LoadPercetage = (double)SongVisualization.EntireSongWaveBuffer.Count / SongManager.Channel32.Length * 4.0;
                                TempRect.X = DurationBar.X + (int)(DurationBar.Width * PlayPercetage);
                                TempRect.Width = (int)(DurationBar.Width * LoadPercetage) - (int)(DurationBar.Width * PlayPercetage);
                                spriteBatch.Draw(Assets.White, TempRect, secondaryColor);
                                if (Config.Data.AntiAliasing)
                                {
                                    TempRect.X = DurationBar.X + (int)(DurationBar.Width * LoadPercetage);
                                    TempRect.Width = 1;
                                    float AAPercentage = (float)(LoadPercetage * DurationBar.Width) % 1;
                                    spriteBatch.Draw(Assets.White, TempRect, secondaryColor * AAPercentage);
                                }
                            }
                            if (Config.Data.AntiAliasing)
                            {
                                TempRect.X = DurationBar.X + (int)(DurationBar.Width * PlayPercetage);
                                TempRect.Width = 1;
                                float AAPercentage = (PlayPercetage * DurationBar.Width) % 1;
                                spriteBatch.Draw(Assets.White, TempRect, primaryColor * AAPercentage);
                            }
                        }
                    }

                    // Second Row
                    if (UpvoteSavedAlpha > 0)
                    {
                        spriteBatch.Draw(Assets.Upvote, Upvote, backgroundColor * UpvoteSavedAlpha);

                        TempVector.X = Upvote.X + Upvote.Width + 3;
                        TempVector.Y = Upvote.Y + Upvote.Height / 2 - 8;
                        spriteBatch.DrawString(Assets.Font, "Upvote saved!", TempVector, backgroundColor * UpvoteSavedAlpha);
                    }
                    else if (SecondRowMessageAlpha > 0)
                    {
                        TempVector.X = 24;
                        TempVector.Y = 45;
                        if (SecondRowMessageAlpha > 1)
                            spriteBatch.DrawString(Assets.Font, SecondRowMessageText, TempVector, backgroundColor);
                        else
                            spriteBatch.DrawString(Assets.Font, SecondRowMessageText, TempVector, backgroundColor * SecondRowMessageAlpha);
                    }

                    // PlayPause Button
                    if (SongManager.IsPlaying())
                    {
                        spriteBatch.Draw(Assets.Pause, PlayPauseButtonShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Pause, PlayPauseButton, backgroundColor);
                    }
                    else
                    {
                        spriteBatch.Draw(Assets.Play, PlayPauseButtonShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Play, PlayPauseButton, backgroundColor);
                    }

                    // Volume
                    if (Values.TargetVolume > MaxVolume * 0.9f)
                    {
                        spriteBatch.Draw(Assets.Volume, VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume, VolumeIcon, backgroundColor);
                    }
                    else if (Values.TargetVolume > MaxVolume * 0.3f)
                    {
                        spriteBatch.Draw(Assets.Volume2, VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume2, VolumeIcon, backgroundColor);
                    }
                    else if (Values.TargetVolume > 0f)
                    {
                        spriteBatch.Draw(Assets.Volume3, VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume3, VolumeIcon, backgroundColor);
                    }
                    else
                    {
                        spriteBatch.Draw(Assets.Volume4, VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume4, VolumeIcon, backgroundColor);
                    }

                    spriteBatch.Draw(Assets.White, VolumeBarShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.White, VolumeBar, backgroundColor);
                    spriteBatch.Draw(Assets.White, TargetVolumeBar, secondaryColor);
                    spriteBatch.Draw(Assets.White, ActualVolumeBar, primaryColor);

                    // UpvoteButton
                    spriteBatch.Draw(Assets.Upvote, UpvoteButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Upvote, UpvoteButton, Color.Lerp(backgroundColor, primaryColor, UpvoteIconAlpha));

                    // CloseButton
                    spriteBatch.Draw(Assets.Close, CloseButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Close, CloseButton, backgroundColor);

                    // OptionsButton
                    spriteBatch.Draw(Assets.Options, OptionsButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Options, OptionsButton, backgroundColor);

                    // Catalyst Grid
                    //for (int x = 1; x < Values.WindowSize.X; x += 2)
                    //    for (int y = 1; y < Values.WindowSize.Y; y += 2)
                    //        spriteBatch.Draw(Assets.White, new Rectangle(x, y, 1, 1), Color.LightGray * 0.2f);

                    //FPSCounter.Draw(spriteBatch);

                    spriteBatch.End();

                    // Title
                    if (TitleTarget != null)
                        lock (TitleTarget)
                        {
                            if (!LongTitle)
                            {
                                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
                                if (TitleTarget != null)
                                {
                                    TempVector.X = 24;
                                    TempVector.Y = 13;
                                    spriteBatch.Draw(TitleTarget, TempVector, backgroundColor);
                                }
                                spriteBatch.End();
                            }
                            else
                            {
                                // Loooong loooooong title man
                                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default,
                                    RasterizerState.CullNone, Assets.TitleFadeout);
                                if (TitleTarget != null)
                                {
                                    TempVector.X = 24;
                                    TempVector.Y = 13;
                                    spriteBatch.Draw(TitleTarget, TempVector, backgroundColor);
                                }
                                spriteBatch.End();
                            }
                        }
                    #endregion
                }
            }

            /*
            VertexPositionColor[] VPC = new VertexPositionColor[3];
            VPC[0] = new VertexPositionColor(new Vector3(100, 200, 0), Color.Red);
            VPC[1] = new VertexPositionColor(new Vector3(Control.GetMouseVector().X, Control.GetMouseVector().Y, 0), Color.Transparent);
            VPC[2] = new VertexPositionColor(new Vector3(100, 300, 0), Color.Red);
            short[] indices = new short[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            Assets.basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, VPC, 0, 3, indices, 0, 1);
            */

            //Debug.WriteLine("Draw: " + (Stopwatch.GetTimestamp() - CurrentDebugTime).ToString());
        }
        public void ForceTitleRedraw(bool clearTarget)
        {
            ForcedTitleRedraw = true;
            if (clearTarget)
                TitleTarget = null;
        }
        public void ForceBackgroundRedraw()
        {
            ForcedBackgroundRedraw = true;
        }
        void BeginBlur()
        {
            GraphicsDevice.SetRenderTarget(TempBlur);
            GraphicsDevice.Clear(Color.Transparent);
        }
        void EndBlur()
        {
            GraphicsDevice.SetRenderTarget(BlurredTex);
            Assets.gaussianBlur.Parameters["BlurWeights"].SetValue(SongVisualization.vBlurWeights);
            Assets.gaussianBlur.Parameters["horz"].SetValue(false);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, Assets.gaussianBlur);
            spriteBatch.Draw(TempBlur, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        void DrawBlurredTex()
        {
            TempVector.X = -50;
            TempVector.Y = -50;
            Assets.gaussianBlur.Parameters["BlurWeights"].SetValue(SongVisualization.hBlurWeights);
            Assets.gaussianBlur.Parameters["horz"].SetValue(true);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, Assets.gaussianBlur);
            spriteBatch.Draw(BlurredTex, TempVector, Color.White);
            spriteBatch.End();
        }
    }
}
