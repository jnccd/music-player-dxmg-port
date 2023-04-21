namespace MusicPlayerDXMonoGamePort
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tSmoothness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ColorChange
            // 
            this.ColorChange.Location = new System.Drawing.Point(7, 22);
            this.ColorChange.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ColorChange.Name = "ColorChange";
            this.ColorChange.Size = new System.Drawing.Size(239, 27);
            this.ColorChange.TabIndex = 0;
            this.ColorChange.Text = "Change primary Color [C]";
            this.ColorChange.UseVisualStyleBackColor = true;
            this.ColorChange.Click += new System.EventHandler(this.ColorChange_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(6, 245);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(413, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 227);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Percentage of future song samples that can be preloaded";
            // 
            // Showinexploerer
            // 
            this.Showinexploerer.Location = new System.Drawing.Point(212, 3);
            this.Showinexploerer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Showinexploerer.Name = "Showinexploerer";
            this.Showinexploerer.Size = new System.Drawing.Size(200, 27);
            this.Showinexploerer.TabIndex = 3;
            this.Showinexploerer.Text = "Show Song File in Explorer [E]";
            this.Showinexploerer.UseVisualStyleBackColor = true;
            this.Showinexploerer.Click += new System.EventHandler(this.button1_Click);
            // 
            // AAtoggle
            // 
            this.AAtoggle.Location = new System.Drawing.Point(7, 55);
            this.AAtoggle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AAtoggle.Name = "AAtoggle";
            this.AAtoggle.Size = new System.Drawing.Size(239, 27);
            this.AAtoggle.TabIndex = 4;
            this.AAtoggle.Text = "Toggle Anti-Aliasing [A]";
            this.AAtoggle.UseVisualStyleBackColor = true;
            this.AAtoggle.Click += new System.EventHandler(this.AAtoggle_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(212, 36);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 27);
            this.button1.TabIndex = 5;
            this.button1.Text = "Reset Music Source Folder";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Reset_Click_1);
            // 
            // ShowStatistics
            // 
            this.ShowStatistics.Location = new System.Drawing.Point(212, 69);
            this.ShowStatistics.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowStatistics.Name = "ShowStatistics";
            this.ShowStatistics.Size = new System.Drawing.Size(200, 27);
            this.ShowStatistics.TabIndex = 6;
            this.ShowStatistics.Text = "Show Statistics [S]";
            this.ShowStatistics.UseVisualStyleBackColor = true;
            this.ShowStatistics.Click += new System.EventHandler(this.ShowStatistics_Click);
            // 
            // ShowConsole
            // 
            this.ShowConsole.Location = new System.Drawing.Point(4, 3);
            this.ShowConsole.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowConsole.Name = "ShowConsole";
            this.ShowConsole.Size = new System.Drawing.Size(200, 27);
            this.ShowConsole.TabIndex = 7;
            this.ShowConsole.Text = "Show Console [K]";
            this.ShowConsole.UseVisualStyleBackColor = true;
            this.ShowConsole.Click += new System.EventHandler(this.ShowConsole_Click);
            // 
            // ShowBrowser
            // 
            this.ShowBrowser.Location = new System.Drawing.Point(4, 69);
            this.ShowBrowser.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowBrowser.Name = "ShowBrowser";
            this.ShowBrowser.Size = new System.Drawing.Size(200, 27);
            this.ShowBrowser.TabIndex = 8;
            this.ShowBrowser.Text = "Show in Browser [I]";
            this.ShowBrowser.UseVisualStyleBackColor = true;
            this.ShowBrowser.Click += new System.EventHandler(this.ShowBrowser_Click);
            // 
            // SwapVisualisations
            // 
            this.SwapVisualisations.Location = new System.Drawing.Point(7, 88);
            this.SwapVisualisations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SwapVisualisations.Name = "SwapVisualisations";
            this.SwapVisualisations.Size = new System.Drawing.Size(239, 27);
            this.SwapVisualisations.TabIndex = 9;
            this.SwapVisualisations.Text = "Swap Visualisations [V]";
            this.SwapVisualisations.UseVisualStyleBackColor = true;
            this.SwapVisualisations.Click += new System.EventHandler(this.SwapVisualisations_Click);
            // 
            // SwapBackgrounds
            // 
            this.SwapBackgrounds.Location = new System.Drawing.Point(7, 121);
            this.SwapBackgrounds.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SwapBackgrounds.Name = "SwapBackgrounds";
            this.SwapBackgrounds.Size = new System.Drawing.Size(239, 27);
            this.SwapBackgrounds.TabIndex = 10;
            this.SwapBackgrounds.Text = "Swap Backgrounds [B]";
            this.SwapBackgrounds.UseVisualStyleBackColor = true;
            this.SwapBackgrounds.Click += new System.EventHandler(this.SwapBackgrounds_Click);
            // 
            // DownloadBox
            // 
            this.DownloadBox.Location = new System.Drawing.Point(7, 62);
            this.DownloadBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DownloadBox.Name = "DownloadBox";
            this.DownloadBox.Size = new System.Drawing.Size(334, 23);
            this.DownloadBox.TabIndex = 11;
            this.DownloadBox.TextChanged += new System.EventHandler(this.DownloadBox_TextChanged);
            this.DownloadBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DownloadBox_KeyDown);
            // 
            // Download
            // 
            this.Download.Location = new System.Drawing.Point(349, 59);
            this.Download.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(63, 27);
            this.Download.TabIndex = 12;
            this.Download.Text = "Start";
            this.Download.UseVisualStyleBackColor = true;
            this.Download.Click += new System.EventHandler(this.Download_Click);
            // 
            // PreloadToggle
            // 
            this.PreloadToggle.Location = new System.Drawing.Point(7, 197);
            this.PreloadToggle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PreloadToggle.Name = "PreloadToggle";
            this.PreloadToggle.Size = new System.Drawing.Size(412, 27);
            this.PreloadToggle.TabIndex = 13;
            this.PreloadToggle.Text = "Enable Preload";
            this.PreloadToggle.UseVisualStyleBackColor = true;
            this.PreloadToggle.Click += new System.EventHandler(this.PreloadToggle_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 14;
            this.label2.Text = "Song Download: ";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ShowProgramFolder
            // 
            this.ShowProgramFolder.Location = new System.Drawing.Point(212, 36);
            this.ShowProgramFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowProgramFolder.Name = "ShowProgramFolder";
            this.ShowProgramFolder.Size = new System.Drawing.Size(200, 27);
            this.ShowProgramFolder.TabIndex = 16;
            this.ShowProgramFolder.Text = "Show Program Folder";
            this.ShowProgramFolder.UseVisualStyleBackColor = true;
            this.ShowProgramFolder.Click += new System.EventHandler(this.ShowProgramFolder_Click);
            // 
            // cAutoVolume
            // 
            this.cAutoVolume.AutoSize = true;
            this.cAutoVolume.Location = new System.Drawing.Point(7, 22);
            this.cAutoVolume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cAutoVolume.Name = "cAutoVolume";
            this.cAutoVolume.Size = new System.Drawing.Size(185, 19);
            this.cAutoVolume.TabIndex = 17;
            this.cAutoVolume.Text = "Real Time Volume Adjustment";
            this.cAutoVolume.UseVisualStyleBackColor = true;
            this.cAutoVolume.CheckedChanged += new System.EventHandler(this.cAutoVolume_CheckedChanged);
            // 
            // bConsoleThreadRestart
            // 
            this.bConsoleThreadRestart.Location = new System.Drawing.Point(212, 3);
            this.bConsoleThreadRestart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bConsoleThreadRestart.Name = "bConsoleThreadRestart";
            this.bConsoleThreadRestart.Size = new System.Drawing.Size(200, 27);
            this.bConsoleThreadRestart.TabIndex = 18;
            this.bConsoleThreadRestart.Text = "Restart Console Input Thread";
            this.bConsoleThreadRestart.UseVisualStyleBackColor = true;
            this.bConsoleThreadRestart.Click += new System.EventHandler(this.bConsoleThreadRestart_Click);
            // 
            // tSmoothness
            // 
            this.tSmoothness.LargeChange = 1;
            this.tSmoothness.Location = new System.Drawing.Point(10, 312);
            this.tSmoothness.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tSmoothness.Maximum = 500;
            this.tSmoothness.Name = "tSmoothness";
            this.tSmoothness.Size = new System.Drawing.Size(409, 45);
            this.tSmoothness.TabIndex = 19;
            this.tSmoothness.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tSmoothness.Value = 50;
            this.tSmoothness.Scroll += new System.EventHandler(this.tSmoothness_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 293);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 15);
            this.label3.TabIndex = 20;
            this.label3.Text = "Diagram Smoothness:";
            // 
            // cOldSmooth
            // 
            this.cOldSmooth.AutoSize = true;
            this.cOldSmooth.Location = new System.Drawing.Point(312, 289);
            this.cOldSmooth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cOldSmooth.Name = "cOldSmooth";
            this.cOldSmooth.Size = new System.Drawing.Size(107, 19);
            this.cOldSmooth.TabIndex = 21;
            this.cOldSmooth.Text = "Old Smoothing";
            this.cOldSmooth.UseVisualStyleBackColor = true;
            this.cOldSmooth.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // bExport
            // 
            this.bExport.Location = new System.Drawing.Point(4, 36);
            this.bExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bExport.Name = "bExport";
            this.bExport.Size = new System.Drawing.Size(200, 27);
            this.bExport.TabIndex = 22;
            this.bExport.Text = "Export Music Library";
            this.bExport.UseVisualStyleBackColor = true;
            this.bExport.Click += new System.EventHandler(this.bExport_Click);
            // 
            // history
            // 
            this.history.Location = new System.Drawing.Point(4, 36);
            this.history.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.history.Name = "history";
            this.history.Size = new System.Drawing.Size(200, 27);
            this.history.TabIndex = 23;
            this.history.Text = "Show Song History";
            this.history.UseVisualStyleBackColor = true;
            this.history.Click += new System.EventHandler(this.history_Click);
            // 
            // bBDownloadF
            // 
            this.bBDownloadF.Location = new System.Drawing.Point(7, 92);
            this.bBDownloadF.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bBDownloadF.Name = "bBDownloadF";
            this.bBDownloadF.Size = new System.Drawing.Size(405, 27);
            this.bBDownloadF.TabIndex = 24;
            this.bBDownloadF.Text = "Add Browser Download Folder so the Browser Extension can work";
            this.bBDownloadF.UseVisualStyleBackColor = true;
            this.bBDownloadF.Click += new System.EventHandler(this.bBDownloadF_Click);
            // 
            // bDiscordRPC
            // 
            this.bDiscordRPC.Location = new System.Drawing.Point(7, 257);
            this.bDiscordRPC.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bDiscordRPC.Name = "bDiscordRPC";
            this.bDiscordRPC.Size = new System.Drawing.Size(239, 27);
            this.bDiscordRPC.TabIndex = 25;
            this.bDiscordRPC.Text = "Activate DiscordRPC";
            this.bDiscordRPC.UseVisualStyleBackColor = true;
            this.bDiscordRPC.Click += new System.EventHandler(this.bDiscordRPC_Click);
            // 
            // cDiscRPC
            // 
            this.cDiscRPC.AutoSize = true;
            this.cDiscRPC.Location = new System.Drawing.Point(7, 232);
            this.cDiscRPC.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cDiscRPC.Name = "cDiscRPC";
            this.cDiscRPC.Size = new System.Drawing.Size(298, 19);
            this.cDiscRPC.TabIndex = 26;
            this.cDiscRPC.Text = "Automatically disable discord RPC on game activity";
            this.cDiscRPC.UseVisualStyleBackColor = true;
            this.cDiscRPC.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // bDrag
            // 
            this.bDrag.Location = new System.Drawing.Point(4, 69);
            this.bDrag.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bDrag.Name = "bDrag";
            this.bDrag.Size = new System.Drawing.Size(200, 27);
            this.bDrag.TabIndex = 27;
            this.bDrag.Text = "DragDrop Song";
            this.bDrag.UseVisualStyleBackColor = true;
            this.bDrag.MouseDown += new System.Windows.Forms.MouseEventHandler(this.bDrag_MouseDown);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(4, 3);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(200, 27);
            this.button2.TabIndex = 28;
            this.button2.Text = "Restart";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(7, 154);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(239, 27);
            this.button3.TabIndex = 29;
            this.button3.Text = "Swap Dark Mode";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // trackBar2
            // 
            this.trackBar2.LargeChange = 1;
            this.trackBar2.Location = new System.Drawing.Point(7, 378);
            this.trackBar2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.trackBar2.Maximum = 15;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(400, 45);
            this.trackBar2.TabIndex = 30;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar2.Value = 5;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.ColorChange);
            this.groupBox1.Controls.Add(this.trackBar2);
            this.groupBox1.Controls.Add(this.AAtoggle);
            this.groupBox1.Controls.Add(this.SwapVisualisations);
            this.groupBox1.Controls.Add(this.SwapBackgrounds);
            this.groupBox1.Controls.Add(this.PreloadToggle);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.trackBar1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tSmoothness);
            this.groupBox1.Controls.Add(this.cOldSmooth);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(426, 430);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Visualization";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 360);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 15);
            this.label4.TabIndex = 31;
            this.label4.Text = "Shadow Distance:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flowLayoutPanel2);
            this.groupBox2.Location = new System.Drawing.Point(444, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(419, 125);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Show";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.flowLayoutPanel1);
            this.groupBox3.Controls.Add(this.cAutoVolume);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.cDiscRPC);
            this.groupBox3.Controls.Add(this.bDiscordRPC);
            this.groupBox3.Controls.Add(this.DownloadBox);
            this.groupBox3.Controls.Add(this.Download);
            this.groupBox3.Controls.Add(this.bBDownloadF);
            this.groupBox3.Location = new System.Drawing.Point(444, 143);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(419, 299);
            this.groupBox3.TabIndex = 33;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Misc";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.bConsoleThreadRestart);
            this.flowLayoutPanel1.Controls.Add(this.bExport);
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.bDrag);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 125);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(419, 101);
            this.flowLayoutPanel1.TabIndex = 25;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.ShowConsole);
            this.flowLayoutPanel2.Controls.Add(this.Showinexploerer);
            this.flowLayoutPanel2.Controls.Add(this.history);
            this.flowLayoutPanel2.Controls.Add(this.ShowProgramFolder);
            this.flowLayoutPanel2.Controls.Add(this.ShowBrowser);
            this.flowLayoutPanel2.Controls.Add(this.ShowStatistics);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 15);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(419, 110);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // OptionsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 453);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "OptionsMenu";
            this.Text = "OptionsMenu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptionsMenu_FormClosed);
            this.Load += new System.EventHandler(this.OptionsMenu_Load);
            this.Shown += new System.EventHandler(this.OptionsMenu_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tSmoothness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}