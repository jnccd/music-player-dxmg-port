﻿namespace MusicPlayerDXMonoGamePort
{
    partial class OptionsMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ColorChange = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.Showinexploerer = new System.Windows.Forms.Button();
            this.AAtoggle = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.ShowStatistics = new System.Windows.Forms.Button();
            this.ShowConsole = new System.Windows.Forms.Button();
            this.ShowBrowser = new System.Windows.Forms.Button();
            this.SwapVisualisations = new System.Windows.Forms.Button();
            this.SwapBackgrounds = new System.Windows.Forms.Button();
            this.DownloadBox = new System.Windows.Forms.TextBox();
            this.Download = new System.Windows.Forms.Button();
            this.PreloadToggle = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ShowProgramFolder = new System.Windows.Forms.Button();
            this.cAutoVolume = new System.Windows.Forms.CheckBox();
            this.bConsoleThreadRestart = new System.Windows.Forms.Button();
            this.tSmoothness = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.cOldSmooth = new System.Windows.Forms.CheckBox();
            this.bExport = new System.Windows.Forms.Button();
            this.history = new System.Windows.Forms.Button();
            this.bBDownloadF = new System.Windows.Forms.Button();
            this.bDiscordRPC = new System.Windows.Forms.Button();
            this.cDiscRPC = new System.Windows.Forms.CheckBox();
            this.bDrag = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tSmoothness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.SuspendLayout();
            // 
            // ColorChange
            // 
            this.ColorChange.Location = new System.Drawing.Point(12, 12);
            this.ColorChange.Name = "ColorChange";
            this.ColorChange.Size = new System.Drawing.Size(205, 23);
            this.ColorChange.TabIndex = 0;
            this.ColorChange.Text = "Change primary Color [C]";
            this.ColorChange.UseVisualStyleBackColor = true;
            this.ColorChange.Click += new System.EventHandler(this.ColorChange_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(12, 230);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(416, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 214);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Percentage of the songs samples that can be preloaded";
            // 
            // Showinexploerer
            // 
            this.Showinexploerer.Location = new System.Drawing.Point(223, 70);
            this.Showinexploerer.Name = "Showinexploerer";
            this.Showinexploerer.Size = new System.Drawing.Size(205, 23);
            this.Showinexploerer.TabIndex = 3;
            this.Showinexploerer.Text = "Show Song File in Explorer [E]";
            this.Showinexploerer.UseVisualStyleBackColor = true;
            this.Showinexploerer.Click += new System.EventHandler(this.button1_Click);
            // 
            // AAtoggle
            // 
            this.AAtoggle.Location = new System.Drawing.Point(223, 12);
            this.AAtoggle.Name = "AAtoggle";
            this.AAtoggle.Size = new System.Drawing.Size(205, 23);
            this.AAtoggle.TabIndex = 4;
            this.AAtoggle.Text = "Toggle Anti-Alising [A]";
            this.AAtoggle.UseVisualStyleBackColor = true;
            this.AAtoggle.Click += new System.EventHandler(this.AAtoggle_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(223, 157);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(205, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Reset Music Source Folder";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Reset_Click_1);
            // 
            // ShowStatistics
            // 
            this.ShowStatistics.Location = new System.Drawing.Point(12, 99);
            this.ShowStatistics.Name = "ShowStatistics";
            this.ShowStatistics.Size = new System.Drawing.Size(205, 23);
            this.ShowStatistics.TabIndex = 6;
            this.ShowStatistics.Text = "Show Statistics [S]";
            this.ShowStatistics.UseVisualStyleBackColor = true;
            this.ShowStatistics.Click += new System.EventHandler(this.ShowStatistics_Click);
            // 
            // ShowConsole
            // 
            this.ShowConsole.Location = new System.Drawing.Point(12, 70);
            this.ShowConsole.Name = "ShowConsole";
            this.ShowConsole.Size = new System.Drawing.Size(205, 23);
            this.ShowConsole.TabIndex = 7;
            this.ShowConsole.Text = "Show Console [K]";
            this.ShowConsole.UseVisualStyleBackColor = true;
            this.ShowConsole.Click += new System.EventHandler(this.ShowConsole_Click);
            // 
            // ShowBrowser
            // 
            this.ShowBrowser.Location = new System.Drawing.Point(223, 99);
            this.ShowBrowser.Name = "ShowBrowser";
            this.ShowBrowser.Size = new System.Drawing.Size(205, 23);
            this.ShowBrowser.TabIndex = 8;
            this.ShowBrowser.Text = "Show in Browser [I]";
            this.ShowBrowser.UseVisualStyleBackColor = true;
            this.ShowBrowser.Click += new System.EventHandler(this.ShowBrowser_Click);
            // 
            // SwapVisualisations
            // 
            this.SwapVisualisations.Location = new System.Drawing.Point(12, 41);
            this.SwapVisualisations.Name = "SwapVisualisations";
            this.SwapVisualisations.Size = new System.Drawing.Size(205, 23);
            this.SwapVisualisations.TabIndex = 9;
            this.SwapVisualisations.Text = "Swap Visualisations [V]";
            this.SwapVisualisations.UseVisualStyleBackColor = true;
            this.SwapVisualisations.Click += new System.EventHandler(this.SwapVisualisations_Click);
            // 
            // SwapBackgrounds
            // 
            this.SwapBackgrounds.Location = new System.Drawing.Point(223, 41);
            this.SwapBackgrounds.Name = "SwapBackgrounds";
            this.SwapBackgrounds.Size = new System.Drawing.Size(205, 23);
            this.SwapBackgrounds.TabIndex = 10;
            this.SwapBackgrounds.Text = "Swap Backgrounds [B]";
            this.SwapBackgrounds.UseVisualStyleBackColor = true;
            this.SwapBackgrounds.Click += new System.EventHandler(this.SwapBackgrounds_Click);
            // 
            // DownloadBox
            // 
            this.DownloadBox.Location = new System.Drawing.Point(12, 294);
            this.DownloadBox.Name = "DownloadBox";
            this.DownloadBox.Size = new System.Drawing.Size(328, 20);
            this.DownloadBox.TabIndex = 11;
            this.DownloadBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DownloadBox_KeyDown);
            // 
            // Download
            // 
            this.Download.Location = new System.Drawing.Point(346, 292);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(82, 23);
            this.Download.TabIndex = 12;
            this.Download.Text = "Start";
            this.Download.UseVisualStyleBackColor = true;
            this.Download.Click += new System.EventHandler(this.Download_Click);
            // 
            // PreloadToggle
            // 
            this.PreloadToggle.Location = new System.Drawing.Point(12, 186);
            this.PreloadToggle.Name = "PreloadToggle";
            this.PreloadToggle.Size = new System.Drawing.Size(416, 23);
            this.PreloadToggle.TabIndex = 13;
            this.PreloadToggle.Text = "Enable Preload";
            this.PreloadToggle.UseVisualStyleBackColor = true;
            this.PreloadToggle.Click += new System.EventHandler(this.PreloadToggle_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 278);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Song Download: ";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ShowProgramFolder
            // 
            this.ShowProgramFolder.Location = new System.Drawing.Point(223, 128);
            this.ShowProgramFolder.Name = "ShowProgramFolder";
            this.ShowProgramFolder.Size = new System.Drawing.Size(205, 23);
            this.ShowProgramFolder.TabIndex = 16;
            this.ShowProgramFolder.Text = "Show Program Folder";
            this.ShowProgramFolder.UseVisualStyleBackColor = true;
            this.ShowProgramFolder.Click += new System.EventHandler(this.ShowProgramFolder_Click);
            // 
            // cAutoVolume
            // 
            this.cAutoVolume.AutoSize = true;
            this.cAutoVolume.Location = new System.Drawing.Point(12, 258);
            this.cAutoVolume.Name = "cAutoVolume";
            this.cAutoVolume.Size = new System.Drawing.Size(167, 17);
            this.cAutoVolume.TabIndex = 17;
            this.cAutoVolume.Text = "Real Time Volume Adjustment";
            this.cAutoVolume.UseVisualStyleBackColor = true;
            this.cAutoVolume.CheckedChanged += new System.EventHandler(this.cAutoVolume_CheckedChanged);
            // 
            // bConsoleThreadRestart
            // 
            this.bConsoleThreadRestart.Location = new System.Drawing.Point(12, 157);
            this.bConsoleThreadRestart.Name = "bConsoleThreadRestart";
            this.bConsoleThreadRestart.Size = new System.Drawing.Size(205, 23);
            this.bConsoleThreadRestart.TabIndex = 18;
            this.bConsoleThreadRestart.Text = "Restart Console Input Thread";
            this.bConsoleThreadRestart.UseVisualStyleBackColor = true;
            this.bConsoleThreadRestart.Click += new System.EventHandler(this.bConsoleThreadRestart_Click);
            // 
            // tSmoothness
            // 
            this.tSmoothness.LargeChange = 1;
            this.tSmoothness.Location = new System.Drawing.Point(12, 337);
            this.tSmoothness.Maximum = 500;
            this.tSmoothness.Name = "tSmoothness";
            this.tSmoothness.Size = new System.Drawing.Size(413, 45);
            this.tSmoothness.TabIndex = 19;
            this.tSmoothness.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tSmoothness.Value = 50;
            this.tSmoothness.Scroll += new System.EventHandler(this.tSmoothness_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 321);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Diagram Smoothness:";
            // 
            // cOldSmooth
            // 
            this.cOldSmooth.AutoSize = true;
            this.cOldSmooth.Location = new System.Drawing.Point(333, 317);
            this.cOldSmooth.Name = "cOldSmooth";
            this.cOldSmooth.Size = new System.Drawing.Size(95, 17);
            this.cOldSmooth.TabIndex = 21;
            this.cOldSmooth.Text = "Old Smoothing";
            this.cOldSmooth.UseVisualStyleBackColor = true;
            this.cOldSmooth.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // bExport
            // 
            this.bExport.Location = new System.Drawing.Point(223, 254);
            this.bExport.Name = "bExport";
            this.bExport.Size = new System.Drawing.Size(205, 23);
            this.bExport.TabIndex = 22;
            this.bExport.Text = "Export Music Library";
            this.bExport.UseVisualStyleBackColor = true;
            this.bExport.Click += new System.EventHandler(this.bExport_Click);
            // 
            // history
            // 
            this.history.Location = new System.Drawing.Point(12, 128);
            this.history.Name = "history";
            this.history.Size = new System.Drawing.Size(205, 23);
            this.history.TabIndex = 23;
            this.history.Text = "Show Song History";
            this.history.UseVisualStyleBackColor = true;
            this.history.Click += new System.EventHandler(this.history_Click);
            // 
            // bBDownloadF
            // 
            this.bBDownloadF.Location = new System.Drawing.Point(12, 360);
            this.bBDownloadF.Name = "bBDownloadF";
            this.bBDownloadF.Size = new System.Drawing.Size(205, 23);
            this.bBDownloadF.TabIndex = 24;
            this.bBDownloadF.Text = "Add Browser Download Folder so the Browser Extension can work";
            this.bBDownloadF.UseVisualStyleBackColor = true;
            this.bBDownloadF.Click += new System.EventHandler(this.bBDownloadF_Click);
            // 
            // bDiscordRPC
            // 
            this.bDiscordRPC.Location = new System.Drawing.Point(12, 412);
            this.bDiscordRPC.Name = "bDiscordRPC";
            this.bDiscordRPC.Size = new System.Drawing.Size(205, 23);
            this.bDiscordRPC.TabIndex = 25;
            this.bDiscordRPC.Text = "Activate DiscordRPC";
            this.bDiscordRPC.UseVisualStyleBackColor = true;
            this.bDiscordRPC.Click += new System.EventHandler(this.bDiscordRPC_Click);
            // 
            // cDiscRPC
            // 
            this.cDiscRPC.AutoSize = true;
            this.cDiscRPC.Location = new System.Drawing.Point(12, 389);
            this.cDiscRPC.Name = "cDiscRPC";
            this.cDiscRPC.Size = new System.Drawing.Size(396, 17);
            this.cDiscRPC.TabIndex = 26;
            this.cDiscRPC.Text = "Automatic DiscordRPC deactivation when playing a game/activation when not";
            this.cDiscRPC.UseVisualStyleBackColor = true;
            this.cDiscRPC.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // bDrag
            // 
            this.bDrag.Location = new System.Drawing.Point(223, 412);
            this.bDrag.Name = "bDrag";
            this.bDrag.Size = new System.Drawing.Size(205, 23);
            this.bDrag.TabIndex = 27;
            this.bDrag.Text = "DragDrop Song";
            this.bDrag.UseVisualStyleBackColor = true;
            this.bDrag.MouseDown += new System.Windows.Forms.MouseEventHandler(this.bDrag_MouseDown);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(223, 360);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(205, 23);
            this.button2.TabIndex = 28;
            this.button2.Text = "Restart";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 441);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(205, 23);
            this.button3.TabIndex = 29;
            this.button3.Text = "Swap Dark Mode";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // trackBar2
            // 
            this.trackBar2.LargeChange = 1;
            this.trackBar2.Location = new System.Drawing.Point(223, 441);
            this.trackBar2.Maximum = 15;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(205, 45);
            this.trackBar2.TabIndex = 30;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar2.Value = 5;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // OptionsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 474);
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.bDrag);
            this.Controls.Add(this.cDiscRPC);
            this.Controls.Add(this.bDiscordRPC);
            this.Controls.Add(this.bBDownloadF);
            this.Controls.Add(this.history);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PreloadToggle);
            this.Controls.Add(this.bExport);
            this.Controls.Add(this.cOldSmooth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bConsoleThreadRestart);
            this.Controls.Add(this.cAutoVolume);
            this.Controls.Add(this.ShowProgramFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Download);
            this.Controls.Add(this.DownloadBox);
            this.Controls.Add(this.SwapBackgrounds);
            this.Controls.Add(this.SwapVisualisations);
            this.Controls.Add(this.ShowBrowser);
            this.Controls.Add(this.ShowConsole);
            this.Controls.Add(this.ShowStatistics);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.AAtoggle);
            this.Controls.Add(this.Showinexploerer);
            this.Controls.Add(this.ColorChange);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.tSmoothness);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "OptionsMenu";
            this.Text = "OptionsMenu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptionsMenu_FormClosed);
            this.Load += new System.EventHandler(this.OptionsMenu_Load);
            this.Shown += new System.EventHandler(this.OptionsMenu_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tSmoothness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ColorChange;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Showinexploerer;
        private System.Windows.Forms.Button AAtoggle;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button ShowStatistics;
        private System.Windows.Forms.Button ShowConsole;
        private System.Windows.Forms.Button ShowBrowser;
        private System.Windows.Forms.Button SwapVisualisations;
        private System.Windows.Forms.Button SwapBackgrounds;
        private System.Windows.Forms.TextBox DownloadBox;
        private System.Windows.Forms.Button Download;
        private System.Windows.Forms.Button PreloadToggle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button ShowProgramFolder;
        private System.Windows.Forms.CheckBox cAutoVolume;
        private System.Windows.Forms.Button bConsoleThreadRestart;
        private System.Windows.Forms.TrackBar tSmoothness;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cOldSmooth;
        private System.Windows.Forms.Button bExport;
        private System.Windows.Forms.Button history;
        private System.Windows.Forms.Button bBDownloadF;
        private System.Windows.Forms.Button bDiscordRPC;
        private System.Windows.Forms.CheckBox cDiscRPC;
        private System.Windows.Forms.Button bDrag;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TrackBar trackBar2;
    }
}