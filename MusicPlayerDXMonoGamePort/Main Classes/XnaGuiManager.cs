﻿using Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Wave;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
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

    public static class XnaGuiManager
    {
        static SelectedControl selectedControl = SelectedControl.None;

        static Form gameWindowForm;

        static System.Drawing.Point MouseClickedPos = new();
        static System.Drawing.Point WindowLocation = new();
        public static System.Drawing.Point newPos = new System.Drawing.Point(Config.Data.WindowPos.X, Config.Data.WindowPos.Y);
        public static System.Drawing.Point oldPos;
        static Vector2 TempVector = new Vector2(0, 0);

        static Task CloseConfirmationThread;
        public static long SkipStartPosition = 0;
        public static long SongTimeSkipped = 0;
        public static bool WasFocusedLastFrame = true;
        static bool wasClickedOn = false;
        public const float MaxVolume = 1f;

        // Draw
        public static Rectangle DurationBar = new Rectangle(51, Values.WindowSize.Y - 28, Values.WindowSize.X - 157, 3);
        public static Rectangle VolumeIcon = new Rectangle(Values.WindowSize.X - 132, 16, 24, 24);
        public static Rectangle VolumeBar = new Rectangle(Values.WindowSize.X - 100, 24, 75, 8);
        public static Rectangle PlayPauseButton = new Rectangle(24, Values.WindowSize.Y - 34, 16, 16);
        public static Rectangle Upvote = new Rectangle(24, 43, 20, 20);
        public static Rectangle TargetVolumeBar = new Rectangle(); // needs Updates
        public static Rectangle ActualVolumeBar = new Rectangle(); // needs Updates
        public static Rectangle UpvoteButton = new Rectangle(Values.WindowSize.X - 98, Values.WindowSize.Y - 35, 19, 19);
        public static Rectangle CloseButton = new Rectangle(Values.WindowSize.X - 43, Values.WindowSize.Y - 34, 18, 18);
        public static Rectangle OptionsButton = new Rectangle(Values.WindowSize.X - 71, Values.WindowSize.Y - 34, 19, 19);

        // Shadows
        public static Rectangle DurationBarShadow;
        public static Rectangle VolumeIconShadow;
        public static Rectangle VolumeBarShadow;
        public static Rectangle PlayPauseButtonShadow;
        public static Rectangle UpvoteShadow;
        public static Rectangle UpvoteButtonShadow;
        public static Rectangle CloseButtonShadow;
        public static Rectangle OptionsButtonShadow;

        // Hitbox Rectangles
        public static Rectangle DurationBarHitbox;
        public static Rectangle VolumeBarHitbox;
        public static Rectangle PlayPauseButtonHitbox;
        public static Rectangle UpvoteButtonHitbox;

        public static void Init(Form gameWindowForm)
        {
            XnaGuiManager.gameWindowForm = gameWindowForm;

            UpdateShadowRects();

            DurationBarHitbox = new Rectangle(DurationBar.X, DurationBar.Y - 10, DurationBar.Width, 23);
            VolumeBarHitbox = new Rectangle(Values.WindowSize.X - 100, 20, 110, 16);
            PlayPauseButtonHitbox = new Rectangle(14, Values.WindowSize.Y - 39, 26, 26);
            UpvoteButtonHitbox = new Rectangle(UpvoteButton.X, UpvoteButton.Y, 20, 20);
        }

        public static void ComputeControls()
        {
            // Mouse Controls
            if (Control.WasLMBJustPressed() && gameWindowForm.Focused &&
                Control.GetMouseRect().Intersects(Values.WindowRect) ||
                !WasFocusedLastFrame && gameWindowForm.Focused &&
                Control.GetMouseRect().Intersects(Values.WindowRect))
            {
                KeyHookManager.CreateGlobalKeyHooks(Program.game.gameWindowForm.Handle);
                MouseClickedPos.X = Control.CurMS.X;
                MouseClickedPos.Y = Control.CurMS.Y;

                if (Values.GetWindow(gameWindowForm.Handle, 2) != Program.game.Shadow.Handle)
                {
                    Program.game.Shadow.BringToFront();
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
                    Program.game.UpdateDiscordRPC();
                }
                selectedControl = SelectedControl.None;
            }
            if (Control.CurMS.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                if (selectedControl == SelectedControl.DragWindow)
                    Program.game.ForceBackgroundRedraw();
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
                        Program.game.ShowOptions();
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

                    //if (newPos.X % 50 == 0)
                    //    GetHashCode();

                    Config.Data.WindowPos = newPos;
                    Program.game.KeepWindowInScreen();
                    Program.game.ForceBackgroundRedraw();

                    if (Program.game.VisSetting == Visualizations.grid)
                    {
                        TempVector.X = oldPos.X - Config.Data.WindowPos.X;
                        TempVector.Y = oldPos.Y - Config.Data.WindowPos.Y;
                        Program.game.DG.ApplyForceGlobally(TempVector);
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
                Program.game.ShowOptions();

            // Swap Visualisations [V]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                Program.game.VisSetting++;
                if ((int)Program.game.VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                    Program.game.VisSetting = 0;

                if (Program.game.VisSetting == Visualizations.dynamicline)
                    Program.game.VisSetting = Visualizations.fft;
            }

            // Swap Backgrounds [B]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                Program.game.BgModes++;
                if ((int)Program.game.BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                    Program.game.BgModes = 0;
                if (Program.game.BgModes == BackGroundModes.None)
                    Program.game.Shadow.Opacity = 0;
                else if (Program.game.BgModes == BackGroundModes.None + 1)
                {
                    Program.game.Shadow.Dispose();
                    Program.game.Shadow = new DropShadow(gameWindowForm, true);
                    Program.game.Shadow.Show();
                    Program.game.Shadow.UpdateSizeLocation();
                    gameWindowForm.BringToFront();
                }
                Program.game.ForceBackgroundRedraw();
            }

            // New Color [C]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.C))
                Program.game.ShowColorDialog();

            // Copy URL the Clipboard [U]
            if (Control.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            {
                Values.StartSTATask(() =>
                {
                    try
                    {
                        Clipboard.SetText(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName).GetYoutubeVideoURL());
                        Program.game.ShowSecondRowMessage("Copied  URL  to  clipboard!", 1);
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
                Program.game.Preload = !Program.game.Preload;
                Program.game.ShowSecondRowMessage("Preload was set to " + Program.game.Preload + " \nThis setting will be applied when the next song starts", 1);
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
            if (Control.CurKS.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
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
                Program.game.ShowStatistics();
        }

        public static void UpdateRectangles()
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
        public static void UpdateShadowRects()
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
    }
}
