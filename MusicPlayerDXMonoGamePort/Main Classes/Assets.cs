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
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Configuration;
using MessageBox = System.Windows.Forms.MessageBox;
using static System.Net.Mime.MediaTypeNames;

namespace MusicPlayerDXMonoGamePort
{
    public static class Assets
    {
        public static SpriteFont Font;
        public static SpriteFont Title;

        public static Texture2D White;
        public static Texture2D bg;
        public static Texture2D Volume;
        public static Texture2D Volume2;
        public static Texture2D Volume3;
        public static Texture2D Volume4;
        public static Texture2D ColorFade;
        public static Texture2D Play;
        public static Texture2D Pause;
        public static Texture2D Upvote;
        public static Texture2D Close;
        public static Texture2D Options;
        public static Texture2D TrumpetBoy;
        public static Texture2D TrumpetBoyBackground;
        public static Texture2D TrumpetBoyTrumpet;
        public static Texture2D CoverPicture;

        public static Color SystemDefaultColor;

        public static Effect gaussianBlur;
        public static Effect PixelBlur;
        public static Effect TitleFadeout;
        public static Effect Vignette;
        public static BasicEffect basicEffect;

        // Loading / Disposing Data
        public static void LoadLoadingScreen(ContentManager Content, GraphicsDevice GD)
        {
            White = new Texture2D(GD, 1, 1);
            Color[] Col = new Color[1];
            Col[0] = Color.White;
            White.SetData(Col);

            gaussianBlur = Content.Load<Effect>("GaussianBlur");
            float bloomStrength = 3;
            GaussianValues.CreateIfNotFilled(bloomStrength);
            SongVisualization.vBlurWeights = Enumerable.Range(-7, 15).Select(x => (float)GaussianValues.GetGaussian(x) / (1 + bloomStrength * 0.08f)).ToArray();
            GaussianValues.CreateIfNotFilled(bloomStrength * 2);
            SongVisualization.hBlurWeights = Enumerable.Range(-7, 15).Select(x => (float)GaussianValues.GetGaussian(x) / (1 + bloomStrength * 0.16f)).ToArray();
            gaussianBlur.Parameters["BlurWeights"].SetValue(SongVisualization.hBlurWeights);
            gaussianBlur.Parameters["InvTexsize"].SetValue(new Vector2(1 / Values.WindowSize.X, 1 / Values.WindowSize.Y));
        }
        public static void Load(ContentManager Content, GraphicsDevice GD)
        {
            Console.WriteLine("Loading Effects...");
            PixelBlur = Content.Load<Effect>("PixelBlur");
            TitleFadeout = Content.Load<Effect>("TitleFadeout");
            Vignette = Content.Load<Effect>("Vignette");
            basicEffect = new BasicEffect(GD);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GD.Viewport.Width, GD.Viewport.Height, 0, 1.0f, 1000.0f);
            basicEffect.VertexColorEnabled = true;
            
            Console.WriteLine("Loading Textures...");
            Color[] Col = new Color[1];
            int res = 8;
            ColorFade = new Texture2D(GD, 1, res);
            Col = new Color[res];
            for (int i = 0; i < Col.Length; i++)
                Col[i] = Color.FromNonPremultiplied(255, 255, 255, (int)(i / (float)res * 255));
            ColorFade.SetData(Col);

            Volume = Content.Load<Texture2D>("volume");
            Volume2 = Content.Load<Texture2D>("volume2");
            Volume3 = Content.Load<Texture2D>("volume3");
            Volume4 = Content.Load<Texture2D>("volume4");
            Play = Content.Load<Texture2D>("play");
            Pause = Content.Load<Texture2D>("pause");
            Upvote = Content.Load<Texture2D>("Upvote");
            Close = Content.Load<Texture2D>("Close");
            Options = Content.Load<Texture2D>("Options");
            TrumpetBoy = Content.Load<Texture2D>("trumpetboy");
            TrumpetBoyBackground = Content.Load<Texture2D>("trumpetboybackground");
            TrumpetBoyTrumpet = Content.Load<Texture2D>("trumpetboytrumpet");

            Console.WriteLine("Loading Fonts...");
            Font = Content.Load<SpriteFont>("Font");
            Title = Content.Load<SpriteFont>("Title");


            Console.WriteLine("Loading Background...");
            RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
            {
                MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
            }
            try
            {
                FileStream Stream = new FileStream(UserWallpaper.GetValue("WallPaper").ToString(), FileMode.Open);
                bg = Texture2D.FromStream(GD, Stream);
                Stream.Dispose();
            }
            catch
            {
                throw new Exception("CouldntFindWallpaperFile");
            }
            SongVisualization.RefreshBGtex(GD);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler((object o, UserPreferenceChangedEventArgs target) =>
            {
                SongVisualization.RefreshBGtex(GD);
                // System Default Color
                int argbColorRefresh = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                System.Drawing.Color tempRefresh = System.Drawing.Color.FromArgb(argbColorRefresh);
                SystemDefaultColor = Color.FromNonPremultiplied(tempRefresh.R, tempRefresh.G, tempRefresh.B, tempRefresh.A);
                
                Program.game.KeepWindowInScreen();
            });
            // System Default Color
            try
            {
                int argbColor = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                System.Drawing.Color temp = System.Drawing.Color.FromArgb(argbColor);
                SystemDefaultColor = Color.FromNonPremultiplied(temp.R, temp.G, temp.B, temp.A);
            }
            catch
            {
                Console.WriteLine("Couldn't find System Default Color!");
                SystemDefaultColor = Color.White;
            }

            Console.WriteLine("Searching for Songs...");
            if (Directory.Exists(Config.Data.MusicPath) && DirOrSubDirsContainMp3(Config.Data.MusicPath))
                FindAllMp3FilesInDir(Config.Data.MusicPath, true);
            else
            {
                FolderBrowserDialog open = new FolderBrowserDialog();
                open.Description = "Select your music folder";
                if (open.ShowDialog() != DialogResult.OK) Process.GetCurrentProcess().Kill();
                Config.Data.MusicPath = open.SelectedPath;
                Config.Save();
                FindAllMp3FilesInDir(open.SelectedPath, true);
            }
            Console.WriteLine();


            if (Config.Data.Col != System.Drawing.Color.Transparent)
            {
                Program.game.primaryColor = Color.FromNonPremultiplied(Config.Data.Col.R, Config.Data.Col.G, Config.Data.Col.B, Config.Data.Col.A);
                Program.game.secondaryColor = Color.Lerp(Program.game.primaryColor, Config.Data.BackgroundColor.ToXNAColor(), 0.4f);
            }
            else
            {
                Program.game.primaryColor = SystemDefaultColor;
                if (Program.game.primaryColor.A != 255) Program.game.primaryColor.A = 255;
                Program.game.secondaryColor = Color.Lerp(Program.game.primaryColor, Program.game.backgroundColor, 0.4f);
            }

            Console.WriteLine("Starting first Song...");
            SongManager.PlayFirstSong();

            Console.WriteLine("Loading GUI...");
            Values.MinimizeConsole();
        }
        public static void FindAllMp3FilesInDir(string StartDir, bool ConsoleOutput)
        {
            foreach (string s in Directory.GetFiles(StartDir))
                if (s.EndsWith(".mp3"))
                {
                    SongManager.RegisterNewSong(s);
                    if (ConsoleOutput)
                    {
                        Console.CursorLeft = 0;
                        Console.Write("Found " + SongManager.Playlist.Count.ToString() + " Songs!");
                    }
                }

            foreach (string D in Directory.GetDirectories(StartDir))
                FindAllMp3FilesInDir(D, ConsoleOutput);
        }
        public static bool DirOrSubDirsContainMp3(string StartDir)
        {
            foreach (string s in Directory.GetFiles(StartDir))
                if (s.EndsWith(".mp3"))
                    return true;

            foreach (string D in Directory.GetDirectories(StartDir))
                if (DirOrSubDirsContainMp3(D))
                    return true;
            return false;
        }
    }
}
