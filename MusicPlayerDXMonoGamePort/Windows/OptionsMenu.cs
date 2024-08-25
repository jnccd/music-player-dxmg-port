﻿using Persistence;
using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
    public partial class OptionsMenu : Form
    {
        public XNA parent;
        bool DownloadFinished;
        bool DoesPreloadActuallyWork = true;
        public bool IsClosed = false;
        public bool HasBeenShown = false;

        public OptionsMenu(XNA parent)
        {
            /*
            this.EnableBlur();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;
            */
            InitializeComponent();
            //FormBorderStyle = FormBorderStyle.None;
            this.parent = parent;
        }

        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            cDiscRPC.Checked = Config.Data.AutoStopDiscordRpcOnGameDetection;
            if (Config.Data.DiscordRpcActive)
                bDiscordRPC.Text = "Deactivate DiscordRPC";
            else
                bDiscordRPC.Text = "Activate DiscordRPC";
            if (Config.Data.BrowserDownloadFolderPath != "" && Config.Data.BrowserDownloadFolderPath != null)
                bBDownloadF.Text = "Change Browser Extension Download Folder";
            trackBar1.Value = Config.Data.WavePreload;
            label1.Text = "Percentage of the songs samples that can be preloaded " + trackBar1.Value + "%";
            if (Program.game.Preload)
            {
                PreloadToggle.Text = "Disable Preload";
                trackBar1.Enabled = true;
                label1.Enabled = true;
            }
            else
            {
                PreloadToggle.Text = "Enable Preload";
                trackBar1.Enabled = false;
                label1.Enabled = false;
            }
            cAutoVolume.Checked = Config.Data.AutoVolume;
            tSmoothness.Value = (int)(Config.Data.Smoothness * 100);
            if (Config.Data.OldSmooth)
            {
                tSmoothness.Enabled = false;
                cOldSmooth.Checked = true;
            }
            else
            {
                tSmoothness.Enabled = true;
                cOldSmooth.Checked = false;
            }
        }

        private void PreloadToggle_Click(object sender, EventArgs e)
        {
            if (DoesPreloadActuallyWork)
            {
                Program.game.Preload = !Program.game.Preload;

                Program.game.ShowSecondRowMessage("Preload was set to " + Program.game.Preload + " \nThis setting will be applied when the next song starts", 1);

                if (Program.game.Preload)
                {
                    PreloadToggle.Text = "Disable Preload";
                    trackBar1.Enabled = true;
                    label1.Enabled = true;
                }
                else
                {
                    PreloadToggle.Text = "Enable Preload";
                    trackBar1.Enabled = false;
                    label1.Enabled = false;
                }
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Config.Data.WavePreload = trackBar1.Value;

            label1.Text = "Percentage of the songs samples that can be preloaded " + trackBar1.Value + "%";
        }
        private void ColorChange_Click(object sender, EventArgs e)
        {
            Program.game.ShowColorDialog();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(SongManager.currentlyPlayingSongPath))
                return;
            else
                Process.Start("explorer.exe", "/select, \"" + SongManager.currentlyPlayingSongPath + "\"");
        }
        private void AAtoggle_Click(object sender, EventArgs e)
        {
            Config.Data.AntiAliasing = !Config.Data.AntiAliasing;
        }
        private void Reset_Click_1(object sender, EventArgs e)
        {
            Program.game.ResetMusicSourcePath();
        }
        private void ShowStatistics_Click(object sender, EventArgs e)
        {
            parent.ShowStatistics();
        }
        private void ShowConsole_Click(object sender, EventArgs e)
        {
            Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
            Values.SetForegroundWindow(Values.GetConsoleWindow());
        }
        private void ShowBrowser_Click(object sender, EventArgs e)
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
                catch (Exception ex) { MessageBox.Show("Can't find that song.\n\nException: " + ex.ToString()); }
            });
        }
        private void SwapVisualisations_Click(object sender, EventArgs e)
        {
            Program.game.VisSetting++;
            if ((int)Program.game.VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                Program.game.VisSetting = 0;

            if (Program.game.VisSetting == Visualizations.dynamicline)
                Program.game.VisSetting = Visualizations.fft;
        }
        private void SwapBackgrounds_Click(object sender, EventArgs e)
        {
            Program.game.BgModes++;
            if ((int)Program.game.BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                Program.game.BgModes = 0;
            Program.game.ForceBackgroundRedraw();
        }

        private void Download_Click(object sender, EventArgs e)
        {
            Values.StartSTATask(() => {
                string download = DownloadBox.Text;
                Program.game.optionsMenu.InvokeIfRequired(() => { Download.Text = "Downloading..."; });
                Program.game.optionsMenu.InvokeIfRequired(() => { Download.Enabled = false; });
                Program.game.optionsMenu.InvokeIfRequired(() => { DownloadBox.Enabled = false; });
                ConsoleManager.PauseConsoleInputThread = true;
                Task T = Task.Factory.StartNew(() => {
                    ConsoleManager.Download(download);
                });
                Thread.Sleep(200);
                Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
                Values.SetForegroundWindow(Values.GetConsoleWindow());
                SendKeys.SendWait(ConsoleManager.wakeUpChar);
                T.Wait();
                Program.game.optionsMenu.InvokeIfRequired(() => { Download.Text = "Start"; });
                Program.game.optionsMenu.InvokeIfRequired(() => { Download.Enabled = true; });
                Program.game.optionsMenu.InvokeIfRequired(() => { DownloadBox.Enabled = true; });
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DownloadFinished)
            {
                DownloadBox.Text = "";
                Download.Text = "Start";
                Download.Enabled = true;
                DownloadBox.Enabled = true;
                DownloadFinished = false;
                ConsoleManager.PauseConsoleInputThread = false;
            }
        }

        private void ShowProgramFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/select, \"" + Values.CurrentExecutablePath + "\"");
        }

        private void cAutoVolume_CheckedChanged(object sender, EventArgs e)
        {
            Config.Data.AutoVolume = cAutoVolume.Checked;
        }

        private void bConsoleThreadRestart_Click(object sender, EventArgs e)
        {
            ConsoleManager.PauseConsoleInputThread = false;
            if (ConsoleManager.IsTaskClosed())
                ConsoleManager.StartSongInputLoop();
        }

        private void tSmoothness_Scroll(object sender, EventArgs e)
        {
            Config.Data.Smoothness = tSmoothness.Value / 100f;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Config.Data.OldSmooth = cOldSmooth.Checked;
            tSmoothness.Enabled = !cOldSmooth.Checked;
        }

        private void OptionsMenu_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsClosed = true;
        }
        
        private void bExport_Click(object sender, EventArgs e)
        {
            ExportsChooser Chooser = new ExportsChooser();
            Chooser.ShowDialog();

            if (Chooser.Output != null)
            {
                FolderBrowserDialog choose = new FolderBrowserDialog();
                DialogResult result = choose.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        if (ConsoleManager.BackgroundOperationRunning || ConsoleManager.ConsoleBackgroundOperationRunning)
                        {
                            MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                            return;
                        }
                        ConsoleManager.BackgroundOperationRunning = true;

                        UpdateExports updat = new UpdateExports(Chooser.Output, choose.SelectedPath);
                        updat.ShowDialog();

                        ConsoleManager.BackgroundOperationRunning = false;
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); ConsoleManager.BackgroundOperationRunning = false; }
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void history_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (parent.history == null || parent.history.IsDisposed)
            //        parent.history = new History();

            //    parent.history.Show();
            //}
            //catch {
            //    parent.history = new History();
            //    parent.history.Show();
            //}
            try { parent.history.Close(); } catch { }
            parent.history = new History(parent);
            parent.history.Show();
        }

        private void bBDownloadF_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.SelectedPath = Config.Data.BrowserDownloadFolderPath;
            DialogResult result = FBD.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Program.crackopenthebois != null)
                    Program.crackopenthebois.Dispose();
                Program.crackopenthebois = new FileSystemWatcher();
                try
                {
                    if (Directory.Exists(FBD.SelectedPath))
                    {
                        Config.Data.BrowserDownloadFolderPath = FBD.SelectedPath;
                        Config.Save();

                        Program.crackopenthebois.Path = Config.Data.BrowserDownloadFolderPath;
                        Program.crackopenthebois.Changed += Program.CrackOpen;
                        Program.crackopenthebois.EnableRaisingEvents = true;
                    }
                    else
                    {
                        MessageBox.Show("Couldn't set filewatcher! (wrong SelectedPath: " + Config.Data.BrowserDownloadFolderPath + " )");
                    }
                }
                catch (Exception ex) { MessageBox.Show("Couldn't set filewatcher! (ERROR: " + ex + ")"); }
                bBDownloadF.Text = "Change Browser Extension Download Folder";
            }
        }

        public void DiscordToggleWrapper()
        {
            bDiscordRPC_Click(null, EventArgs.Empty);
        }
        private void bDiscordRPC_Click(object sender, EventArgs e)
        {
            if (bDiscordRPC.Enabled)
            {
                bDiscordRPC.Enabled = false;
                Config.Data.DiscordRpcActive = !Config.Data.DiscordRpcActive;

                if (Config.Data.DiscordRpcActive)
                {
                    DiscordRPCWrapper.Initialize("460490126607384576");
                    Program.game.UpdateDiscordRPC();
                    bDiscordRPC.Text = "Deactivate DiscordRPC";
                }
                else
                {
                    DiscordRPCWrapper.Shutdown();
                    bDiscordRPC.Text = "Activate DiscordRPC";
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1500);
                    Program.game.optionsMenu.InvokeIfRequired(() => { bDiscordRPC.Enabled = true; });
                });
            }
        }

        private void DownloadBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Download_Click(null, EventArgs.Empty);
        }

        private void OptionsMenu_Shown(object sender, EventArgs e)
        {
            HasBeenShown = true;
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Data.AutoStopDiscordRpcOnGameDetection = cDiscRPC.Checked;
        }

        private void bDrag_MouseDown(object sender, MouseEventArgs e)
        {
            string path = SongManager.currentlyPlayingSongPath;
            string[] files = new string[1]; files[0] = path;
            bDrag.DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Values.StartSTATask(() =>
            {
                try
                {
                    if (MessageBox.Show("Do you really want to restart?", "Restart?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Program.closing = true;
                        Program.game.gameWindowForm.InvokeIfRequired(Program.game.gameWindowForm.Close);
                        DiscordRPCWrapper.Shutdown();
                        Application.Exit();
                        Program.Restart();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Can't restart.\n\nException: " + ex.ToString()); }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Program.game.backgroundColor == Microsoft.Xna.Framework.Color.White)
            {
                Program.game.backgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(51, 51, 51, 255);
                Program.game.secondaryColor = Microsoft.Xna.Framework.Color.Lerp(Program.game.primaryColor, Program.game.backgroundColor, 0.4f);
            }
            else
            {
                Program.game.backgroundColor = Microsoft.Xna.Framework.Color.White;
                Program.game.secondaryColor = Microsoft.Xna.Framework.Color.Lerp(Program.game.primaryColor, Program.game.backgroundColor, 0.4f);
            }
            Config.Data.BackgroundColor = Color.FromArgb(Program.game.backgroundColor.R, Program.game.backgroundColor.G, Program.game.backgroundColor.B, Program.game.backgroundColor.A);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            Config.Data.ShadowDistance = trackBar2.Value;
            XnaGuiManager.UpdateRectangles();
            XnaGuiManager.UpdateShadowRects();
            Program.game.ForceTitleRedraw(false);
        }

        private void DownloadBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        bool keyhookActivated = true;
        private void bKeyhook_Click(object sender, EventArgs e)
        {
            if (keyhookActivated)
                KeyHookManager.DisposeGlobalKeyHooks();
            else
                KeyHookManager.CreateGlobalKeyHooks(Program.game.gameWindowForm.Handle);
            keyhookActivated = !keyhookActivated;
        }
    }
}
