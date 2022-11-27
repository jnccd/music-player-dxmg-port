﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
    public partial class UpdateMetadata : Form
    {
        string[] SongPaths;
        bool closing = false;

        public UpdateMetadata(string[] SongPaths)
        {
            InitializeComponent();
            this.Text = "Updating Mp3-Song-Metadata";
            this.SongPaths = SongPaths;
        }

        private void UpdateMetadata_Load(object sender, EventArgs e)
        {

        }

        private void UpdateMetadata_Shown(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < SongPaths.Length; i++)
            {
                backgroundWorker1.ReportProgress(i);
                string name = Path.GetFileNameWithoutExtension(SongPaths[i]);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                for (int j = 0; j < 3; j++)
                {
                    try
                    {
                        HttpWebRequest req = null;
                        WebResponse W = null;

                        string videoID = name.GetYoutubeVideoID();
                        string ResultURL = videoID.GetYoutubeVideoURLFromID();
                        string thumbURL = videoID.GetYoutubeThumbnail();
                        string artist = name.Contains("-") ? name.Split('-').First().Trim() : "Unknown";

                        // edit mp3 metadata using taglib
                        HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(thumbURL);
                        HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        Stream stream = httpWebReponse.GetResponseStream();
                        Image im = Image.FromStream(stream);
                        TagLib.File file = TagLib.File.Create(SongPaths[i]);
                        TagLib.Picture pic = new TagLib.Picture();
                        pic.Type = TagLib.PictureType.FrontCover;
                        pic.Description = "Cover";
                        pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        MemoryStream ms = new MemoryStream();
                        im.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Position = 0;
                        pic.Data = TagLib.ByteVector.FromStream(ms);
                        file.Tag.Pictures = new TagLib.IPicture[] { pic };
                        file.Tag.Performers = new string[] { artist };
                        file.Tag.Comment = "Downloaded using MusicPlayer";
                        file.Tag.Album = name;
                        file.Tag.AlbumArtists = new string[] { artist };
                        file.Tag.Artists = new string[] { artist };
                        file.Tag.AmazonId = "AmazonIsShit";
                        file.Tag.Composers = new string[] { artist };
                        file.Tag.Copyright = "None";
                        file.Tag.Disc = 1;
                        file.Tag.DiscCount = 1;
                        file.Tag.Genres = new string[] { "good music" };
                        file.Tag.Grouping = "None";
                        file.Tag.Lyrics = "You expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\n";
                        file.Tag.MusicIpId = "wubbel";
                        file.Tag.Title = name;
                        file.Tag.Track = 1;
                        file.Tag.TrackCount = 1;
                        file.Tag.Year = 1982;

                        file.Save();
                        ms.Close();

                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error processing: " + name + "\nError Message: " + ex.Message);
                    }
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = (int)(e.ProgressPercentage / (float)SongPaths.Length * 100);
            progressLabel.Text = progressBar.Value + " % " + Path.GetFileNameWithoutExtension(SongPaths[e.ProgressPercentage]);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closing = true;
            this.Close();
        }

        private void UpdateMetadata_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closing == false)
                e.Cancel = true;
        }
    }
}
