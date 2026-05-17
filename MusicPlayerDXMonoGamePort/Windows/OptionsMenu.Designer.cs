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
            components = new System.ComponentModel.Container();
            ColorChange = new System.Windows.Forms.Button();
            trackBar1 = new System.Windows.Forms.TrackBar();
            label1 = new System.Windows.Forms.Label();
            Showinexploerer = new System.Windows.Forms.Button();
            AAtoggle = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            ShowStatistics = new System.Windows.Forms.Button();
            ShowConsole = new System.Windows.Forms.Button();
            ShowBrowser = new System.Windows.Forms.Button();
            SwapVisualisations = new System.Windows.Forms.Button();
            SwapBackgrounds = new System.Windows.Forms.Button();
            DownloadBox = new System.Windows.Forms.TextBox();
            Download = new System.Windows.Forms.Button();
            PreloadToggle = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            ShowProgramFolder = new System.Windows.Forms.Button();
            cAutoVolume = new System.Windows.Forms.CheckBox();
            bConsoleThreadRestart = new System.Windows.Forms.Button();
            tSmoothness = new System.Windows.Forms.TrackBar();
            label3 = new System.Windows.Forms.Label();
            cOldSmooth = new System.Windows.Forms.CheckBox();
            bExport = new System.Windows.Forms.Button();
            history = new System.Windows.Forms.Button();
            bBDownloadF = new System.Windows.Forms.Button();
            bDiscordRPC = new System.Windows.Forms.Button();
            cDiscRPC = new System.Windows.Forms.CheckBox();
            bDrag = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            trackBar2 = new System.Windows.Forms.TrackBar();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label4 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            groupBox3 = new System.Windows.Forms.GroupBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            bKeyhook = new System.Windows.Forms.Button();
            groupBox4 = new System.Windows.Forms.GroupBox();
            buttonRegister = new System.Windows.Forms.Button();
            textBoxConnectionState = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            textBoxPassword = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            textBoxUsername = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            buttonLogin = new System.Windows.Forms.Button();
            textBoxHost = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tSmoothness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            groupBox3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // ColorChange
            // 
            ColorChange.Location = new System.Drawing.Point(13, 47);
            ColorChange.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            ColorChange.Name = "ColorChange";
            ColorChange.Size = new System.Drawing.Size(444, 58);
            ColorChange.TabIndex = 0;
            ColorChange.Text = "Change primary Color [C]";
            ColorChange.UseVisualStyleBackColor = true;
            ColorChange.Click += ColorChange_Click;
            // 
            // trackBar1
            // 
            trackBar1.Location = new System.Drawing.Point(11, 785);
            trackBar1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            trackBar1.Maximum = 100;
            trackBar1.Minimum = 1;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(767, 90);
            trackBar1.TabIndex = 1;
            trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar1.Value = 50;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 747);
            label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(627, 32);
            label1.TabIndex = 2;
            label1.Text = "Percentage of future song samples that can be preloaded";
            // 
            // Showinexploerer
            // 
            Showinexploerer.Location = new System.Drawing.Point(392, 6);
            Showinexploerer.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            Showinexploerer.Name = "Showinexploerer";
            Showinexploerer.Size = new System.Drawing.Size(371, 58);
            Showinexploerer.TabIndex = 3;
            Showinexploerer.Text = "Show Song File in Explorer [E]";
            Showinexploerer.UseVisualStyleBackColor = true;
            Showinexploerer.Click += button1_Click;
            // 
            // AAtoggle
            // 
            AAtoggle.Location = new System.Drawing.Point(13, 117);
            AAtoggle.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            AAtoggle.Name = "AAtoggle";
            AAtoggle.Size = new System.Drawing.Size(444, 58);
            AAtoggle.TabIndex = 4;
            AAtoggle.Text = "Toggle Anti-Aliasing [A]";
            AAtoggle.UseVisualStyleBackColor = true;
            AAtoggle.Click += AAtoggle_Click;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(391, 76);
            button1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(370, 58);
            button1.TabIndex = 5;
            button1.Text = "Reset Music Source Folder";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Reset_Click_1;
            // 
            // ShowStatistics
            // 
            ShowStatistics.Location = new System.Drawing.Point(392, 146);
            ShowStatistics.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            ShowStatistics.Name = "ShowStatistics";
            ShowStatistics.Size = new System.Drawing.Size(371, 58);
            ShowStatistics.TabIndex = 6;
            ShowStatistics.Text = "Show Statistics [S]";
            ShowStatistics.UseVisualStyleBackColor = true;
            ShowStatistics.Click += ShowStatistics_Click;
            // 
            // ShowConsole
            // 
            ShowConsole.Location = new System.Drawing.Point(7, 6);
            ShowConsole.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            ShowConsole.Name = "ShowConsole";
            ShowConsole.Size = new System.Drawing.Size(371, 58);
            ShowConsole.TabIndex = 7;
            ShowConsole.Text = "Show Console [K]";
            ShowConsole.UseVisualStyleBackColor = true;
            ShowConsole.Click += ShowConsole_Click;
            // 
            // ShowBrowser
            // 
            ShowBrowser.Location = new System.Drawing.Point(7, 146);
            ShowBrowser.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            ShowBrowser.Name = "ShowBrowser";
            ShowBrowser.Size = new System.Drawing.Size(371, 58);
            ShowBrowser.TabIndex = 8;
            ShowBrowser.Text = "Show in Browser [I]";
            ShowBrowser.UseVisualStyleBackColor = true;
            ShowBrowser.Click += ShowBrowser_Click;
            // 
            // SwapVisualisations
            // 
            SwapVisualisations.Location = new System.Drawing.Point(13, 188);
            SwapVisualisations.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            SwapVisualisations.Name = "SwapVisualisations";
            SwapVisualisations.Size = new System.Drawing.Size(444, 58);
            SwapVisualisations.TabIndex = 9;
            SwapVisualisations.Text = "Swap Visualisations [V]";
            SwapVisualisations.UseVisualStyleBackColor = true;
            SwapVisualisations.Click += SwapVisualisations_Click;
            // 
            // SwapBackgrounds
            // 
            SwapBackgrounds.Location = new System.Drawing.Point(13, 258);
            SwapBackgrounds.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            SwapBackgrounds.Name = "SwapBackgrounds";
            SwapBackgrounds.Size = new System.Drawing.Size(444, 58);
            SwapBackgrounds.TabIndex = 10;
            SwapBackgrounds.Text = "Swap Backgrounds [B]";
            SwapBackgrounds.UseVisualStyleBackColor = true;
            SwapBackgrounds.Click += SwapBackgrounds_Click;
            // 
            // DownloadBox
            // 
            DownloadBox.Location = new System.Drawing.Point(13, 132);
            DownloadBox.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            DownloadBox.Name = "DownloadBox";
            DownloadBox.Size = new System.Drawing.Size(583, 39);
            DownloadBox.TabIndex = 11;
            DownloadBox.TextChanged += DownloadBox_TextChanged;
            DownloadBox.KeyDown += DownloadBox_KeyDown;
            // 
            // Download
            // 
            Download.Location = new System.Drawing.Point(615, 126);
            Download.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            Download.Name = "Download";
            Download.Size = new System.Drawing.Size(150, 58);
            Download.TabIndex = 12;
            Download.Text = "Start";
            Download.UseVisualStyleBackColor = true;
            Download.Click += Download_Click;
            // 
            // PreloadToggle
            // 
            PreloadToggle.Location = new System.Drawing.Point(13, 683);
            PreloadToggle.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            PreloadToggle.Name = "PreloadToggle";
            PreloadToggle.Size = new System.Drawing.Size(765, 58);
            PreloadToggle.TabIndex = 13;
            PreloadToggle.Text = "Enable Preload";
            PreloadToggle.UseVisualStyleBackColor = true;
            PreloadToggle.Click += PreloadToggle_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 94);
            label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(196, 32);
            label2.TabIndex = 14;
            label2.Text = "Song Download: ";
            label2.Click += label2_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            // 
            // ShowProgramFolder
            // 
            ShowProgramFolder.Location = new System.Drawing.Point(392, 76);
            ShowProgramFolder.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            ShowProgramFolder.Name = "ShowProgramFolder";
            ShowProgramFolder.Size = new System.Drawing.Size(371, 58);
            ShowProgramFolder.TabIndex = 16;
            ShowProgramFolder.Text = "Show Program Folder";
            ShowProgramFolder.UseVisualStyleBackColor = true;
            ShowProgramFolder.Click += ShowProgramFolder_Click;
            // 
            // cAutoVolume
            // 
            cAutoVolume.AutoSize = true;
            cAutoVolume.Location = new System.Drawing.Point(13, 47);
            cAutoVolume.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            cAutoVolume.Name = "cAutoVolume";
            cAutoVolume.Size = new System.Drawing.Size(368, 36);
            cAutoVolume.TabIndex = 17;
            cAutoVolume.Text = "Real Time Volume Adjustment";
            cAutoVolume.UseVisualStyleBackColor = true;
            cAutoVolume.CheckedChanged += cAutoVolume_CheckedChanged;
            // 
            // bConsoleThreadRestart
            // 
            bConsoleThreadRestart.Location = new System.Drawing.Point(391, 6);
            bConsoleThreadRestart.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bConsoleThreadRestart.Name = "bConsoleThreadRestart";
            bConsoleThreadRestart.Size = new System.Drawing.Size(370, 58);
            bConsoleThreadRestart.TabIndex = 18;
            bConsoleThreadRestart.Text = "Restart Console Input Thread";
            bConsoleThreadRestart.UseVisualStyleBackColor = true;
            bConsoleThreadRestart.Click += bConsoleThreadRestart_Click;
            // 
            // tSmoothness
            // 
            tSmoothness.LargeChange = 1;
            tSmoothness.Location = new System.Drawing.Point(19, 437);
            tSmoothness.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            tSmoothness.Maximum = 500;
            tSmoothness.Name = "tSmoothness";
            tSmoothness.Size = new System.Drawing.Size(760, 90);
            tSmoothness.TabIndex = 19;
            tSmoothness.TickStyle = System.Windows.Forms.TickStyle.None;
            tSmoothness.Value = 50;
            tSmoothness.Scroll += tSmoothness_Scroll;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(11, 397);
            label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(247, 32);
            label3.TabIndex = 20;
            label3.Text = "Diagram Smoothness:";
            // 
            // cOldSmooth
            // 
            cOldSmooth.AutoSize = true;
            cOldSmooth.Location = new System.Drawing.Point(578, 395);
            cOldSmooth.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            cOldSmooth.Name = "cOldSmooth";
            cOldSmooth.Size = new System.Drawing.Size(209, 36);
            cOldSmooth.TabIndex = 21;
            cOldSmooth.Text = "Old Smoothing";
            cOldSmooth.UseVisualStyleBackColor = true;
            cOldSmooth.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // bExport
            // 
            bExport.Location = new System.Drawing.Point(7, 76);
            bExport.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bExport.Name = "bExport";
            bExport.Size = new System.Drawing.Size(370, 58);
            bExport.TabIndex = 22;
            bExport.Text = "Export Music Library";
            bExport.UseVisualStyleBackColor = true;
            bExport.Click += bExport_Click;
            // 
            // history
            // 
            history.Location = new System.Drawing.Point(7, 76);
            history.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            history.Name = "history";
            history.Size = new System.Drawing.Size(371, 58);
            history.TabIndex = 23;
            history.Text = "Show Song History";
            history.UseVisualStyleBackColor = true;
            history.Click += history_Click;
            // 
            // bBDownloadF
            // 
            bBDownloadF.Location = new System.Drawing.Point(13, 196);
            bBDownloadF.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bBDownloadF.Name = "bBDownloadF";
            bBDownloadF.Size = new System.Drawing.Size(752, 58);
            bBDownloadF.TabIndex = 24;
            bBDownloadF.Text = "Add Browser Download Folder so the Browser Extension can work";
            bBDownloadF.UseVisualStyleBackColor = true;
            bBDownloadF.Click += bBDownloadF_Click;
            // 
            // bDiscordRPC
            // 
            bDiscordRPC.Location = new System.Drawing.Point(13, 548);
            bDiscordRPC.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bDiscordRPC.Name = "bDiscordRPC";
            bDiscordRPC.Size = new System.Drawing.Size(368, 58);
            bDiscordRPC.TabIndex = 25;
            bDiscordRPC.Text = "Activate DiscordRPC";
            bDiscordRPC.UseVisualStyleBackColor = true;
            bDiscordRPC.Click += bDiscordRPC_Click;
            // 
            // cDiscRPC
            // 
            cDiscRPC.AutoSize = true;
            cDiscRPC.Location = new System.Drawing.Point(13, 495);
            cDiscRPC.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            cDiscRPC.Name = "cDiscRPC";
            cDiscRPC.Size = new System.Drawing.Size(590, 36);
            cDiscRPC.TabIndex = 26;
            cDiscRPC.Text = "Automatically disable discord RPC on game activity";
            cDiscRPC.UseVisualStyleBackColor = true;
            cDiscRPC.CheckedChanged += checkBox1_CheckedChanged_1;
            // 
            // bDrag
            // 
            bDrag.Location = new System.Drawing.Point(7, 146);
            bDrag.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bDrag.Name = "bDrag";
            bDrag.Size = new System.Drawing.Size(370, 58);
            bDrag.TabIndex = 27;
            bDrag.Text = "DragDrop Song";
            bDrag.UseVisualStyleBackColor = true;
            bDrag.MouseDown += bDrag_MouseDown;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(7, 6);
            button2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(370, 58);
            button2.TabIndex = 28;
            button2.Text = "Restart";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(13, 329);
            button3.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(444, 58);
            button3.TabIndex = 29;
            button3.Text = "Swap Dark Mode";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // trackBar2
            // 
            trackBar2.LargeChange = 1;
            trackBar2.Location = new System.Drawing.Point(13, 578);
            trackBar2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            trackBar2.Maximum = 15;
            trackBar2.Name = "trackBar2";
            trackBar2.Size = new System.Drawing.Size(763, 90);
            trackBar2.TabIndex = 30;
            trackBar2.TickStyle = System.Windows.Forms.TickStyle.None;
            trackBar2.Value = 5;
            trackBar2.Scroll += trackBar2_Scroll;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = System.Drawing.SystemColors.Control;
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(button3);
            groupBox1.Controls.Add(ColorChange);
            groupBox1.Controls.Add(PreloadToggle);
            groupBox1.Controls.Add(trackBar1);
            groupBox1.Controls.Add(trackBar2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(AAtoggle);
            groupBox1.Controls.Add(SwapVisualisations);
            groupBox1.Controls.Add(SwapBackgrounds);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(tSmoothness);
            groupBox1.Controls.Add(cOldSmooth);
            groupBox1.Location = new System.Drawing.Point(22, 26);
            groupBox1.Margin = new System.Windows.Forms.Padding(6);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(6);
            groupBox1.Size = new System.Drawing.Size(791, 917);
            groupBox1.TabIndex = 31;
            groupBox1.TabStop = false;
            groupBox1.Text = "Visualization";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(19, 540);
            label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(201, 32);
            label4.TabIndex = 31;
            label4.Text = "Shadow Distance:";
            // 
            // groupBox2
            // 
            groupBox2.BackColor = System.Drawing.SystemColors.Control;
            groupBox2.Controls.Add(flowLayoutPanel2);
            groupBox2.Location = new System.Drawing.Point(825, 26);
            groupBox2.Margin = new System.Windows.Forms.Padding(6);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(6);
            groupBox2.Size = new System.Drawing.Size(778, 267);
            groupBox2.TabIndex = 32;
            groupBox2.TabStop = false;
            groupBox2.Text = "Show";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(ShowConsole);
            flowLayoutPanel2.Controls.Add(Showinexploerer);
            flowLayoutPanel2.Controls.Add(history);
            flowLayoutPanel2.Controls.Add(ShowProgramFolder);
            flowLayoutPanel2.Controls.Add(ShowBrowser);
            flowLayoutPanel2.Controls.Add(ShowStatistics);
            flowLayoutPanel2.Location = new System.Drawing.Point(2, 32);
            flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(6);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(774, 230);
            flowLayoutPanel2.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.BackColor = System.Drawing.SystemColors.Control;
            groupBox3.Controls.Add(flowLayoutPanel1);
            groupBox3.Controls.Add(cAutoVolume);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(cDiscRPC);
            groupBox3.Controls.Add(bDiscordRPC);
            groupBox3.Controls.Add(DownloadBox);
            groupBox3.Controls.Add(Download);
            groupBox3.Controls.Add(bBDownloadF);
            groupBox3.Location = new System.Drawing.Point(825, 305);
            groupBox3.Margin = new System.Windows.Forms.Padding(6);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(6);
            groupBox3.Size = new System.Drawing.Size(778, 638);
            groupBox3.TabIndex = 33;
            groupBox3.TabStop = false;
            groupBox3.Text = "Misc";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(button2);
            flowLayoutPanel1.Controls.Add(bConsoleThreadRestart);
            flowLayoutPanel1.Controls.Add(bExport);
            flowLayoutPanel1.Controls.Add(button1);
            flowLayoutPanel1.Controls.Add(bDrag);
            flowLayoutPanel1.Controls.Add(bKeyhook);
            flowLayoutPanel1.Location = new System.Drawing.Point(6, 267);
            flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(769, 215);
            flowLayoutPanel1.TabIndex = 25;
            flowLayoutPanel1.Paint += flowLayoutPanel1_Paint;
            // 
            // bKeyhook
            // 
            bKeyhook.Location = new System.Drawing.Point(391, 146);
            bKeyhook.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            bKeyhook.Name = "bKeyhook";
            bKeyhook.Size = new System.Drawing.Size(370, 58);
            bKeyhook.TabIndex = 29;
            bKeyhook.Text = "Toggle Global Keyhooks";
            bKeyhook.UseVisualStyleBackColor = true;
            bKeyhook.Click += bKeyhook_Click;
            // 
            // groupBox4
            // 
            groupBox4.BackColor = System.Drawing.SystemColors.Control;
            groupBox4.Controls.Add(buttonRegister);
            groupBox4.Controls.Add(textBoxConnectionState);
            groupBox4.Controls.Add(label7);
            groupBox4.Controls.Add(textBoxPassword);
            groupBox4.Controls.Add(label6);
            groupBox4.Controls.Add(textBoxUsername);
            groupBox4.Controls.Add(label5);
            groupBox4.Controls.Add(buttonLogin);
            groupBox4.Controls.Add(textBoxHost);
            groupBox4.Location = new System.Drawing.Point(23, 955);
            groupBox4.Margin = new System.Windows.Forms.Padding(6);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(6);
            groupBox4.Size = new System.Drawing.Size(1580, 162);
            groupBox4.TabIndex = 34;
            groupBox4.TabStop = false;
            groupBox4.Text = "Sync Server Connection";
            // 
            // buttonRegister
            // 
            buttonRegister.Location = new System.Drawing.Point(1417, 46);
            buttonRegister.Name = "buttonRegister";
            buttonRegister.Size = new System.Drawing.Size(150, 46);
            buttonRegister.TabIndex = 36;
            buttonRegister.Text = "Register";
            buttonRegister.UseVisualStyleBackColor = true;
            buttonRegister.Click += buttonRegister_Click;
            // 
            // textBoxConnectionState
            // 
            textBoxConnectionState.Location = new System.Drawing.Point(10, 106);
            textBoxConnectionState.Multiline = true;
            textBoxConnectionState.Name = "textBoxConnectionState";
            textBoxConnectionState.Size = new System.Drawing.Size(1388, 39);
            textBoxConnectionState.TabIndex = 35;
            textBoxConnectionState.Text = "State:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(1076, 53);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(116, 32);
            label7.TabIndex = 7;
            label7.Text = "Password:";
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new System.Drawing.Point(1198, 50);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new System.Drawing.Size(200, 39);
            textBoxPassword.TabIndex = 6;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(738, 51);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(126, 32);
            label6.TabIndex = 5;
            label6.Text = "Username:";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new System.Drawing.Point(870, 48);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new System.Drawing.Size(200, 39);
            textBoxUsername.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(9, 48);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(68, 32);
            label5.TabIndex = 3;
            label5.Text = "Host:";
            // 
            // buttonLogin
            // 
            buttonLogin.Location = new System.Drawing.Point(1417, 102);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.Size = new System.Drawing.Size(150, 46);
            buttonLogin.TabIndex = 2;
            buttonLogin.Text = "Login";
            buttonLogin.UseVisualStyleBackColor = true;
            buttonLogin.Click += buttonLogin_Click;
            // 
            // textBoxHost
            // 
            textBoxHost.Location = new System.Drawing.Point(83, 45);
            textBoxHost.Name = "textBoxHost";
            textBoxHost.Size = new System.Drawing.Size(384, 39);
            textBoxHost.TabIndex = 1;
            // 
            // OptionsMenu
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1629, 1142);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            MaximizeBox = false;
            Name = "OptionsMenu";
            Text = "OptionsMenu";
            FormClosed += OptionsMenu_FormClosed;
            Load += OptionsMenu_Load;
            Shown += OptionsMenu_Shown;
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ((System.ComponentModel.ISupportInitialize)tSmoothness).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);

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
        private System.Windows.Forms.Button bKeyhook;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.TextBox textBoxHost;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxConnectionState;
        private System.Windows.Forms.Button buttonRegister;
    }
}