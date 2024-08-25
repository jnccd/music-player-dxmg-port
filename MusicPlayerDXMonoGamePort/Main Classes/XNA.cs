using System;
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
using Persistence;
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

    public class XNA : Game
    {
        // Graphics
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public Form gameWindowForm;
        public RenderTarget2D TempBlur;
        public RenderTarget2D BlurredTex;
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
        public DropShadow Shadow;
        bool LongTitle;
        static RenderTarget2D TitleTarget;
        static RenderTarget2D BackgroundTarget;

        // Temp
        static Vector2 TempVector = new Vector2(0, 0);
        static Rectangle TempRect = new Rectangle(0, 0, 0, 0);
        Point Diff = new Point();

        // Stuff
        float SecondRowMessageAlpha;
        string SecondRowMessageText;
        public float UpvoteSavedAlpha = 0;
        float UpvoteIconAlpha = 0;
        List<float> DebugPercentages = new List<float>();
        public bool FocusWindow = false;
        public bool Preload;
        float[] values;
        public Thread SongBufferLoaderThread;
        public Thread MainThread;
        Task SongCheckThread;
        int lastSongRequestCheck = -100;
        string lastQuestionResult = null;
        bool ForcedTitleRedraw = false;
        bool ForcedBackgroundRedraw = false;
        public bool ForcedCoverBackgroundRedraw = false;
        Point[] WindowPoints = new Point[4];
        string Title;
        float X1;
        float X2;
        int ScrollWheelCooldown = 0;
        
        // Forms
        public OptionsMenu optionsMenu;
        public Statistics statistics;
        public History history;
        
        public XNA()
        {
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
            XnaGuiManager.Init(gameWindowForm);
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
                KeyHookManager.DisposeGlobalKeyHooks();
                SongManager.DisposeNAudioData();
                SongManager.SaveUserSettings(true);
                if (optionsMenu != null)
                    optionsMenu.InvokeIfRequired(optionsMenu.Close);
                if (statistics != null)
                    statistics.InvokeIfRequired(statistics.Close);
            };
            Console.CancelKeyPress += ConsoleManager.HandleCancelKeyPress;

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
            ConsoleManager.StartSongInputLoop();

            ShowSecondRowMessage("Found  " + SongManager.Playlist.Count + "  Songs!", 3);

            KeyHookManager.CreateGlobalKeyHooks(Program.game.gameWindowForm.Handle);

            KeepWindowInScreen();
            Shadow = new DropShadow(gameWindowForm, true);
            Shadow.Show();
        }

        // Update
        protected override void Update(GameTime gameTime)
        {
            //CurrentDebugTime = Stopwatch.GetTimestamp();
            //FPSCounter.Update(gameTime);
            Control.Update();
            if (gameWindowForm.Focused)
                XnaGuiManager.ComputeControls();
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

            XnaGuiManager.UpdateRectangles();
            XnaGuiManager.WasFocusedLastFrame = gameWindowForm.Focused;

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
                if (XnaGuiManager.oldPos.X == 0 && XnaGuiManager.oldPos.Y == 0)
                    XnaGuiManager.oldPos = XnaGuiManager.newPos;

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
                        TempVector.X = XnaGuiManager.Upvote.X + XnaGuiManager.Upvote.Width + 3 + Config.Data.ShadowDistance;
                        TempVector.Y = XnaGuiManager.Upvote.Y + XnaGuiManager.Upvote.Height / 2 - 8 + Config.Data.ShadowDistance;
                        spriteBatch.Draw(Assets.Upvote, XnaGuiManager.UpvoteShadow, Color.Black * 0.6f * UpvoteSavedAlpha);
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
                    spriteBatch.Draw(Assets.White, XnaGuiManager.DurationBarShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.White, XnaGuiManager.DurationBar, backgroundColor);
                    if (SongManager.Channel32 != null)
                    {
                        lock (SongManager.Channel32)
                        {
                            float PlayPercetage = (SongManager.Channel32.Position / (float)SongManager.Channel32.WaveFormat.AverageBytesPerSecond /
                                ((float)SongManager.Channel32.TotalTime.TotalSeconds));
                            TempRect.X = XnaGuiManager.DurationBar.X;
                            TempRect.Y = XnaGuiManager.DurationBar.Y;
                            TempRect.Width = (int)(XnaGuiManager.DurationBar.Width * PlayPercetage);
                            TempRect.Height = 3;
                            spriteBatch.Draw(Assets.White, TempRect, primaryColor);
                            if (SongVisualization.EntireSongWaveBuffer != null && Config.Data.Preload)
                            {
                                double LoadPercetage = (double)SongVisualization.EntireSongWaveBuffer.Count / SongManager.Channel32.Length * 4.0;
                                TempRect.X = XnaGuiManager.DurationBar.X + (int)(XnaGuiManager.DurationBar.Width * PlayPercetage);
                                TempRect.Width = (int)(XnaGuiManager.DurationBar.Width * LoadPercetage) - (int)(XnaGuiManager.DurationBar.Width * PlayPercetage);
                                spriteBatch.Draw(Assets.White, TempRect, secondaryColor);
                                if (Config.Data.AntiAliasing)
                                {
                                    TempRect.X = XnaGuiManager.DurationBar.X + (int)(XnaGuiManager.DurationBar.Width * LoadPercetage);
                                    TempRect.Width = 1;
                                    float AAPercentage = (float)(LoadPercetage * XnaGuiManager.DurationBar.Width) % 1;
                                    spriteBatch.Draw(Assets.White, TempRect, secondaryColor * AAPercentage);
                                }
                            }
                            if (Config.Data.AntiAliasing)
                            {
                                TempRect.X = XnaGuiManager.DurationBar.X + (int)(XnaGuiManager.DurationBar.Width * PlayPercetage);
                                TempRect.Width = 1;
                                float AAPercentage = (PlayPercetage * XnaGuiManager.DurationBar.Width) % 1;
                                spriteBatch.Draw(Assets.White, TempRect, primaryColor * AAPercentage);
                            }
                        }
                    }

                    // Second Row
                    if (UpvoteSavedAlpha > 0)
                    {
                        spriteBatch.Draw(Assets.Upvote, XnaGuiManager.Upvote, backgroundColor * UpvoteSavedAlpha);

                        TempVector.X = XnaGuiManager.Upvote.X + XnaGuiManager.Upvote.Width + 3;
                        TempVector.Y = XnaGuiManager.Upvote.Y + XnaGuiManager.Upvote.Height / 2 - 8;
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
                        spriteBatch.Draw(Assets.Pause, XnaGuiManager.PlayPauseButtonShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Pause, XnaGuiManager.PlayPauseButton, backgroundColor);
                    }
                    else
                    {
                        spriteBatch.Draw(Assets.Play, XnaGuiManager.PlayPauseButtonShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Play, XnaGuiManager.PlayPauseButton, backgroundColor);
                    }

                    // Volume
                    if (Values.TargetVolume > XnaGuiManager.MaxVolume * 0.9f)
                    {
                        spriteBatch.Draw(Assets.Volume, XnaGuiManager.VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume, XnaGuiManager.VolumeIcon, backgroundColor);
                    }
                    else if (Values.TargetVolume > XnaGuiManager.MaxVolume * 0.3f)
                    {
                        spriteBatch.Draw(Assets.Volume2, XnaGuiManager.VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume2, XnaGuiManager.VolumeIcon, backgroundColor);
                    }
                    else if (Values.TargetVolume > 0f)
                    {
                        spriteBatch.Draw(Assets.Volume3, XnaGuiManager.VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume3, XnaGuiManager.VolumeIcon, backgroundColor);
                    }
                    else
                    {
                        spriteBatch.Draw(Assets.Volume4, XnaGuiManager.VolumeIconShadow, Color.Black * 0.6f);
                        spriteBatch.Draw(Assets.Volume4, XnaGuiManager.VolumeIcon, backgroundColor);
                    }

                    spriteBatch.Draw(Assets.White, XnaGuiManager.VolumeBarShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.White, XnaGuiManager.VolumeBar, backgroundColor);
                    spriteBatch.Draw(Assets.White, XnaGuiManager.TargetVolumeBar, secondaryColor);
                    spriteBatch.Draw(Assets.White, XnaGuiManager.ActualVolumeBar, primaryColor);

                    // UpvoteButton
                    spriteBatch.Draw(Assets.Upvote, XnaGuiManager.UpvoteButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Upvote, XnaGuiManager.UpvoteButton, Color.Lerp(backgroundColor, primaryColor, UpvoteIconAlpha));

                    // CloseButton
                    spriteBatch.Draw(Assets.Close, XnaGuiManager.CloseButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Close, XnaGuiManager.CloseButton, backgroundColor);

                    // OptionsButton
                    spriteBatch.Draw(Assets.Options, XnaGuiManager.OptionsButtonShadow, Color.Black * 0.6f);
                    spriteBatch.Draw(Assets.Options, XnaGuiManager.OptionsButton, backgroundColor);

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
