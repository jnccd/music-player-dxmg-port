using MusicPlayerDXMonoGamePort.Persistence.Database;
using MusicPlayerSyncInterface.Database;
using MusicPlayerSyncInterface.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicPlayerDXMonoGamePort.Main_Classes;
using Newtonsoft.Json;
using Persistence;

namespace MusicPlayerDXMonoGamePort
{
    public partial class Statistics : Form
    {
        XNA parent;
        int currentMouseOverRow;
        public bool IsClosed = false;
        string LastSearched = "";
        int timerTicks = 0;
        Point MousePos;
        Point MouseDrag;

        public struct DistancePerSong
        {
            public int SongIndex;
            public float SongDifference;
        }

        public Statistics(XNA parent)
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
            this.parent = parent;
        }
        private void Statistics_Load(object sender, EventArgs e)
        {
            bRefresh_Click(this, EventArgs.Empty);
        }

        // Button Events
        private void bRefresh_Click(object sender, EventArgs e)
        {
            int RowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            dataGridView1.Rows.Clear();
            object[] o = new object[7];
            object[,] SongInfo = SongManager.GetSongInformationList();

            using var songDbContext = new SongDbContext();
            for (int i = 0; i < songDbContext.UpvotedSongs.Count(); i++)
            {
                o[0] = SongInfo[i, 0];
                o[1] = SongInfo[i, 1];
                o[2] = SongInfo[i, 2];
                o[3] = SongInfo[i, 3];
                o[4] = SongInfo[i, 4];
                o[5] = SongInfo[i, 5];
                o[6] = SongInfo[i, 6];
                dataGridView1.Rows.Add(o);
                if (o[o.Length - 1] == null)
                    dataGridView1.Rows[dataGridView1.RowCount - 1].DefaultCellStyle.BackColor = Color.Red;
            }

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            for (int i = 1; i < dataGridView1.Columns.Count - 1; i++)
                if (i == 2)
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                else
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].Width = 2;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                if (SongManager.currentlyPlayingSongName.Equals(dataGridView1.Rows[i].Cells[0].Value))
                {
                    dataGridView1.Rows[i].Selected = true;
                    int heightInRows = dataGridView1.Height / dataGridView1.Rows[0].Height;
                    int index = i - heightInRows / 2 + 2;
                    if (index < 0)
                        index = 0;
                }

            if (dataGridView1.SortOrder != SortOrder.None)
            {
                if (dataGridView1.SortedColumn.Index == 7)
                {
                    textBox1.Text = LastSearched;
                    bSearch_Click(null, EventArgs.Empty);
                }
                else
                    dataGridView1.Sort(dataGridView1.SortedColumn, dataGridView1.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            }
            if (RowIndex != -1)
                dataGridView1.FirstDisplayedScrollingRowIndex = RowIndex;
        }
        private void bSearch_Click(object sender, EventArgs e)
        {
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending); //randomly sorted lists will have random search orders for hits with the same score

            string Path = textBox1.Text;
            LastSearched = Path;
            textBox1.Text = "";

            DistancePerSong[] LDistances = new DistancePerSong[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                LDistances[i].SongDifference = Values.LevenshteinDistanceWrapper(Path, ((string)(dataGridView1.Rows[i].Cells[0].Value)));
                LDistances[i].SongIndex = i;
                dataGridView1.Rows[i].Cells[dataGridView1.Rows[i].Cells.Count - 1].Value = LDistances[i].SongDifference;
            }

            dataGridView1.ClearSelection();
            dataGridView1.Sort(dataGridView1.Columns[dataGridView1.Columns.Count - 1], ListSortDirection.Ascending);
            dataGridView1.FirstDisplayedScrollingRowIndex = 0;
        }
        private void toPlaying_Click(object sender, EventArgs e)
        {
            toSong(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName));
        }

        // Data Grid View Events
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            timerTicks = 0;
            timer1.Enabled = true;
            MouseDrag = new Point(e.X, e.Y);
        }
        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            MousePos = new Point(e.X, e.Y);
        }
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.RowIndex >= 0 && !SongManager.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() + ".mp3"))
                    MessageBox.Show("This entry isnt linked to a mp3 file!");
            }
        }

        // ContextMenu
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Up / Downvote Menu
            if (e != null && e.Button == MouseButtons.Right && e.RowIndex == -1 && e.ColumnIndex == 3)
            {
                //ContextMenu m = new ContextMenu();
                //m.MenuItems.Add(new MenuItem("Sort by Upvotes", ((object s, EventArgs ev) =>
                //{
                //    try
                //    {
                //        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                //        {
                //            dataGridView1.Rows[i].Cells[dataGridView1.Rows[i].Cells.Count - 1].Value = Convert.ToInt32(((string)dataGridView1.Rows[i].Cells[3].Value).Split('/').First());
                //        }

                //        dataGridView1.ClearSelection();
                //        dataGridView1.Sort(dataGridView1.Columns[dataGridView1.Columns.Count - 1], ListSortDirection.Descending);
                //        dataGridView1.FirstDisplayedScrollingRowIndex = 0;
                //    }
                //    catch { }
                //})));
                //m.MenuItems.Add(new MenuItem("Sort by Downvotes", ((object s, EventArgs ev) =>
                //{
                //    try
                //    {
                //        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                //        {
                //            dataGridView1.Rows[i].Cells[dataGridView1.Rows[i].Cells.Count - 1].Value = Convert.ToInt32(((string)dataGridView1.Rows[i].Cells[3].Value).Split('/').Last().Split('=').First());
                //        }

                //        dataGridView1.ClearSelection();
                //        dataGridView1.Sort(dataGridView1.Columns[dataGridView1.Columns.Count - 1], ListSortDirection.Descending);
                //        dataGridView1.FirstDisplayedScrollingRowIndex = 0;
                //    }
                //    catch { }
                //})));
                //m.MenuItems.Add(new MenuItem("Sort by Ratio", ((object s, EventArgs ev) =>
                //{
                //    try
                //    {
                //        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                //        {
                //            dataGridView1.Rows[i].Cells[dataGridView1.Rows[i].Cells.Count - 1].Value = Convert.ToSingle(((string)dataGridView1.Rows[i].Cells[3].Value).Split('=').Last());
                //        }

                //        dataGridView1.ClearSelection();
                //        dataGridView1.Sort(dataGridView1.Columns[dataGridView1.Columns.Count - 1], ListSortDirection.Descending);
                //        dataGridView1.FirstDisplayedScrollingRowIndex = 0;
                //    }
                //    catch { }
                //})));
                //m.Show(dataGridView1, new Point(e.X + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, true).X, e.Y));
            }

            // Normal Context Menu
            if (e != null && e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                for (int x = 0; x < dataGridView1.RowCount; x++)
                    for (int y = 0; y < dataGridView1.ColumnCount; y++)
                    {
                        if (dataGridView1.SelectedRows.Contains(dataGridView1.Rows[x]))
                            continue;
                        dataGridView1.Rows[x].Cells[y].Selected = false;
                    }

                dataGridView1.Rows[e.RowIndex].Cells[0].Selected = true;

                ContextMenuStrip m = new ContextMenuStrip();
                m.Items.Add(new ToolStripMenuItem("Play", null, (object s, EventArgs ev) =>
                {
                    try
                    {
                        if (!SongManager.PlayPlaylistSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString() + ".mp3"))
                            MessageBox.Show("This entry isnt linked to a mp3 file!");
                    }
                    catch { }
                }));
                m.Items.Add(new ToolStripMenuItem("Queue", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        SongManager.QueueNewSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString(), false);
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Copy Title to Clipboard", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Clipboard.SetText(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Copy URL to Clipboard", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Clipboard.SetText(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().GetYoutubeVideoURL());
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Open in Browser", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().GetYoutubeVideoURL())
                            {
                                UseShellExecute = true
                            }
                        }.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!");
                    }
                })));
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Equals(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName)))
                    m.Items.Add(new ToolStripMenuItem("Open in Browser with timestamp", null, ((object s, EventArgs ev) =>
                    {
                        try
                        {
                            Task.Factory.StartNew(() =>
                            {
                                int seconds = (int)(SongManager.Channel32.Position / (double)SongManager.Channel32.Length * SongManager.Channel32.TotalTime.TotalSeconds);
                                Uri U = new Uri(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().GetYoutubeVideoURL() + "&t=" + seconds + "s");
                                new Process
                                {
                                    StartInfo = new ProcessStartInfo(U.ToString())
                                    {
                                        UseShellExecute = true
                                    }
                                }.Start();

                                if (SongManager.IsPlaying())
                                    SongManager.PlayPause();
                            });
                        }
                        catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                    })));
                m.Items.Add(new ToolStripMenuItem("Open in Explorer", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        string path = SongManager.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        if (!File.Exists(path))
                            return;
                        else
                            Process.Start("explorer.exe", "/select, \"" + path + "\"");
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Reset Volume Multiplier", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        using var songDbContext = new SongDbContext();
                        var upvotedSong = songDbContext.UpvotedSongs.FirstOrDefault(x => x.Name == dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString() + ".mp3");
                        if (upvotedSong != null)
                        {
                            upvotedSong.Volume = -1;
                            SongManager.SaveUserSettings(false);
                            bRefresh_Click(null, EventArgs.Empty);
                        }
                        songDbContext.SaveChanges();
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Rename", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        string path = SongManager.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());

                        if (!File.Exists(path))
                        {
                            MessageBox.Show("This entry isnt linked to a mp3 file!");
                            return;
                        }

                        if (dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().Equals(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName)))
                        {
                            MessageBox.Show("Sorry Dave but im afraight I cant do that\n(You cant play a file and rename it at the same time!)");
                            return;
                        }

                        stringDialog Dia = new("What name should it get?", dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        Dia.ShowDialog();
                        if (Dia.result == dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString())
                        {
                            MessageBox.Show("You didn't change the name...");
                        }
                        else if (Dia.result != "")
                        {
                            string oldName = dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString() + ".mp3";
                            string newName = Dia.result.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ? Dia.result : Dia.result + ".mp3";

                            // A migration refers to one specific upvotedSong entry (its SongId), so resolve which
                            // entry the user means first. When several entries share the file name they are
                            // different songs; the entry is then disambiguated via the album/artist tags of the
                            // file this row is linked to.
                            Guid targetSongId;
                            List<string> filesToRename;
                            int skippedCopies = 0;
                            using (var preflightDb = new SongDbContext())
                            {
                                var rowsWithThisName = preflightDb.UpvotedSongs.Where(x => x.Name == oldName).ToArray();
                                if (rowsWithThisName.Length == 0)
                                {
                                    MessageBox.Show("No song entry with this name was found in the database!");
                                    return;
                                }

                                UpvotedSong targetEntry;
                                if (rowsWithThisName.Length == 1)
                                {
                                    targetEntry = rowsWithThisName[0];
                                }
                                else
                                {
                                    // Several entries share the file name: pick the one whose metadata matches the
                                    // file this row is linked to. Anything else is ambiguous.
                                    UpvotedSong matchedEntry = null;
                                    foreach (var candidate in rowsWithThisName)
                                    {
                                        if (SyncManager.SongFileMatchesEntry(path, candidate))
                                        {
                                            if (matchedEntry != null)
                                            {
                                                matchedEntry = null; // Several entries match: ambiguous
                                                break;
                                            }
                                            matchedEntry = candidate;
                                        }
                                    }
                                    if (matchedEntry == null)
                                    {
                                        MessageBox.Show("Several songs share this file name and it is not clear which one you want to rename.\n\n" +
                                            "Update the album/artist metadata of the affected songs so they can be told apart, and try again.");
                                        return;
                                    }
                                    targetEntry = matchedEntry;
                                }
                                targetSongId = targetEntry.SongId;

                                // Rename only files whose tags match this entry (a file with the same name but
                                // different album/artist tags is a different song). Entries without album/artist
                                // metadata (legacy entries) can only be identified by their file name.
                                if (string.IsNullOrWhiteSpace(targetEntry.Artist) && string.IsNullOrWhiteSpace(targetEntry.Album))
                                {
                                    filesToRename = SyncManager.FindSongFilesByName(Config.Data.MusicPath, oldName);
                                }
                                else
                                {
                                    filesToRename = new List<string>();
                                    foreach (string candidate in SyncManager.FindSongFilesByName(Config.Data.MusicPath, oldName))
                                    {
                                        if (SyncManager.SongFileMatchesEntry(candidate, targetEntry))
                                            filesToRename.Add(candidate);
                                        else
                                            skippedCopies++;
                                    }
                                }
                            }

                            if (filesToRename.Count == 0)
                            {
                                MessageBox.Show("No file of this song could be found in the song library.\n\n" +
                                    (skippedCopies > 0
                                        ? "Files with this name exist, but their album/artist metadata does not match this song entry, so they were not renamed."
                                        : "Files with this name exist, but they could not be read or matched to this song entry."));
                                return;
                            }

                            // Commit point: the migration POST on the server. The server assigns the migration number
                            // and renames the entry with the given SongId. If this fails, abort without changing
                            // anything locally, since migrations should only be done with a working server connection.
                            var createdMigration = SyncManager.PostSongLibraryMigration(new SongLibraryMigration(oldName, newName, SongLibraryMigrationType.Rename)
                            {
                                SongId = targetSongId
                            });
                            if (createdMigration == null)
                            {
                                MessageBox.Show("Rename aborted! The sync server did not accept the rename.\n\n" + SyncManager.State +
                                    "\n\n(Songs can only be renamed while the connection to the sync server is up and their entry was synced)");
                                return;
                            }

                            // Rename every copy of the song in the library (there can be copies in multiple
                            // subfolders). If a copy is already gone and its target already exists, another client
                            // (e.g. sharing the library via NAS) already renamed it - that counts as done. Only if
                            // all renames went through is the migration state bumped, so a failed rename is going
                            // to be retried automatically on the next startup.
                            List<(string OldPath, string NewPath)> movedFiles = new List<(string, string)>();
                            try
                            {
                                foreach (string oldFilePath in filesToRename)
                                {
                                    string destPath = Path.Combine(Path.GetDirectoryName(oldFilePath) ?? "", newName);
                                    if (File.Exists(destPath))
                                    {
                                        if (File.Exists(oldFilePath))
                                        {
                                            // A different file with the target name is in the way.
                                            RollbackFileRenames(movedFiles);
                                            MessageBox.Show("A file called \"" + newName + "\" already exists in the song library!");
                                            return;
                                        }
                                        movedFiles.Add((oldFilePath, destPath)); // Another client already renamed this copy
                                        continue;
                                    }
                                    File.Move(oldFilePath, destPath);
                                    movedFiles.Add((oldFilePath, destPath));
                                }
                            }
                            catch (Exception ex)
                            {
                                // The migration is already on the server, but the library migration state was not bumped,
                                // so the remaining renames are going to be retried automatically on the next startup.
                                MessageBox.Show("The server registered the rename, but some files could not be renamed locally:\n" + ex.Message);
                                return;
                            }

                            // Update the playlist & play history (the files moved)
                            foreach (var moved in movedFiles)
                            {
                                for (int i = 0; i < SongManager.Playlist.Count; i++)
                                    if (SongManager.Playlist[i] == moved.OldPath)
                                        SongManager.Playlist[i] = moved.NewPath;
                                for (int i = 0; i < SongManager.PlayerHistory.Count; i++)
                                    if (SongManager.PlayerHistory[i] == moved.OldPath)
                                        SongManager.PlayerHistory[i] = moved.NewPath;
                            }

                            // Update the local database entry (the server already renamed its copy of the row).
                            // Also rename queued uploads of the affected song ("/sync/new-song" queue bodies still
                            // contain the old name), so an upload that is retried later creates the row with the
                            // new name instead of resurrecting the old one.
                            using (var songDbContext = new SongDbContext())
                            {
                                var songIdsToRename = new[] { targetSongId };
                                var upvotedSongsToRename = songDbContext.UpvotedSongs.Where(x => x.SongId == targetSongId).ToArray();
                                foreach (var upvotedSong in upvotedSongsToRename)
                                {
                                    if (songDbContext.UpvotedSongs.Any(x => x.SongId != upvotedSong.SongId && x.Name == newName && x.Artist == upvotedSong.Artist && x.Album == upvotedSong.Album))
                                    {
                                        // Target name already taken by a different entry, roll the file renames back.
                                        RollbackFileRenames(movedFiles);
                                        MessageBox.Show("A song with that name already exists!");
                                        return;
                                    }
                                    upvotedSong.Name = newName;
                                }

                                var queuedUploadsToRename = songDbContext.NotYetSyncedData
                                    .Where(x => x.Endpoint == "/sync/new-song" && x.BelongedToSongId != null && songIdsToRename.Contains(x.BelongedToSongId.Value)).ToArray();
                                foreach (var queuedUpload in queuedUploadsToRename)
                                {
                                    var queuedSong = JsonConvert.DeserializeObject<UpvotedSong>(queuedUpload.Body);
                                    if (queuedSong == null)
                                        continue;
                                    queuedSong.Name = newName;
                                    queuedUpload.Body = JsonConvert.SerializeObject(queuedSong, Formatting.Indented);
                                }

                                try
                                {
                                    songDbContext.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    RollbackFileRenames(movedFiles);
                                    MessageBox.Show("Could not update the local database:\n" + ex.Message);
                                    return;
                                }
                            }

                            SyncManager.WriteSongLibraryMigrationState(Config.Data.MusicPath, createdMigration.UserId, createdMigration.MigrationNumber);

                            // If the library turned out to be registered for a different account, warn about it
                            var ownerWarning = SyncManager.TakeSongLibraryOwnerWarning();
                            if (ownerWarning != null)
                                MessageBox.Show(ownerWarning, "Song Library");

                            SongManager.CreateSongChoosingList();
                            bRefresh_Click(null, EventArgs.Empty);

                            MessageBox.Show("Successfully renamed \"" + oldName + "\" to \"" + newName + "\"!" + (skippedCopies > 0
                                ? "\n\nNote: " + skippedCopies + " file(s) with the old name were left alone, since their album/artist metadata did not match this song entry."
                                : ""));
                        }
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Update Mp3-Metadata of Row-Selection", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        if (ConsoleManager.BackgroundOperationRunning || ConsoleManager.ConsoleBackgroundOperationRunning)
                        {
                            MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                            return;
                        }

                        ConsoleManager.BackgroundOperationRunning = true;

                        List<string> SongPaths = new List<string>();
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            if (dataGridView1.Rows[i].Selected)
                                SongPaths.Add(SongManager.GetSongPathFromSongName((string)dataGridView1.Rows[i].Cells[0].Value));
                        UpdateMetadata updat = new UpdateMetadata(SongPaths.ToArray());

                        if (SongPaths.Count > 0)
                            updat.ShowDialog();
                        else
                            MessageBox.Show("You havent selected anything!\nMake sure to select entire Rows");

                        ConsoleManager.BackgroundOperationRunning = false;
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Show Cover Picture", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        string path = SongManager.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        TagLib.File file = TagLib.File.Create(path);
                        TagLib.IPicture pic = file.Tag.Pictures[0];
                        MemoryStream ms = new MemoryStream(pic.Data.Data);
                        if (ms != null && ms.Length > 4096)
                        {
                            Image currentImage = Image.FromStream(ms);
                            path = Values.CurrentExecutablePath + "\\Downloads\\Thumbnail.png";
                            currentImage.Save(path);
                            new Process
                            {
                                StartInfo = new ProcessStartInfo(path)
                                {
                                    UseShellExecute = true
                                }
                            }.Start();
                        }
                        ms.Close();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!\n" + e.ToString());
                    }
                })));
                m.Items.Add(new ToolStripMenuItem("Filter for...", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        stringDialog dia = new stringDialog("What do you want to filter for?", "");
                        dia.ShowDialog();
                        if (dia.result != "" && dia.result != null)
                            filterFor(dia.result);
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.Items.Add(new ToolStripMenuItem("Delete Entry", null, ((object s, EventArgs ev) =>
                {
                    try
                    {
                        string songName = dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString() + ".mp3";

                        if (dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().Equals(Path.GetFileNameWithoutExtension(SongManager.currentlyPlayingSongName)))
                        {
                            MessageBox.Show("Sorry Dave but im afraight I cant do that\n(You cant play a file and delete it at the same time!)");
                            return;
                        }

                        if (MessageBox.Show("Do you really want to delete \"" + songName + "\"?\n\n" +
                            "This deletes the song entry AND the song file from the database and from all synchronized song libraries!",
                            "Delete Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;

                        // A migration refers to one specific upvotedSong entry (its SongId), so resolve which
                        // entry the user means first (same logic as in the rename flow).
                        Guid targetSongId;
                        List<string> filesToDelete = new List<string>();
                        int skippedFiles = 0;
                        using (var preflightDb = new SongDbContext())
                        {
                            var rowsWithThisName = preflightDb.UpvotedSongs.Where(x => x.Name == songName).ToArray();
                            if (rowsWithThisName.Length == 0)
                            {
                                MessageBox.Show("No song entry with this name was found in the database!");
                                return;
                            }

                            UpvotedSong targetEntry;
                            if (rowsWithThisName.Length == 1)
                            {
                                targetEntry = rowsWithThisName[0];
                            }
                            else
                            {
                                // Several entries share the file name: pick the one whose metadata matches the
                                // linked file. Anything else is ambiguous.
                                string linkedFilePath = SongManager.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                                UpvotedSong matchedEntry = null;
                                foreach (var candidate in rowsWithThisName)
                                {
                                    if (File.Exists(linkedFilePath) && SyncManager.SongFileMatchesEntry(linkedFilePath, candidate))
                                    {
                                        if (matchedEntry != null)
                                        {
                                            matchedEntry = null; // Several entries match: ambiguous
                                            break;
                                        }
                                        matchedEntry = candidate;
                                    }
                                }
                                if (matchedEntry == null)
                                {
                                    MessageBox.Show("Several songs share this file name and it is not clear which one you want to delete.\n\n" +
                                        "Update the album/artist metadata of the affected songs so they can be told apart, and try again.");
                                    return;
                                }
                                targetEntry = matchedEntry;
                            }
                            targetSongId = targetEntry.SongId;

                            // Only delete files whose tags match this entry (a file with the same name but
                            // different album/artist tags is a different song). Entries without album/artist
                            // metadata can only be identified by their file name.
                            if (Directory.Exists(Config.Data.MusicPath))
                            {
                                if (string.IsNullOrWhiteSpace(targetEntry.Artist) && string.IsNullOrWhiteSpace(targetEntry.Album))
                                {
                                    filesToDelete = SyncManager.FindSongFilesByName(Config.Data.MusicPath, songName);
                                }
                                else
                                {
                                    foreach (string candidate in SyncManager.FindSongFilesByName(Config.Data.MusicPath, songName))
                                    {
                                        if (SyncManager.SongFileMatchesEntry(candidate, targetEntry))
                                            filesToDelete.Add(candidate);
                                        else
                                            skippedFiles++;
                                    }
                                }
                            }
                        }

                        // Commit point: the migration POST on the server. The server assigns the migration number
                        // and removes the entry with the given SongId (and its history entries). If this fails,
                        // abort without changing anything locally, since migrations should only be done with a
                        // working server connection.
                        var createdMigration = SyncManager.PostSongLibraryMigration(new SongLibraryMigration(songName, "", SongLibraryMigrationType.Delete)
                        {
                            SongId = targetSongId
                        });
                        if (createdMigration == null)
                        {
                            MessageBox.Show("Delete aborted! The sync server did not accept the deletion.\n\n" + SyncManager.State +
                                "\n\n(Songs can only be deleted while the connection to the sync server is up and their entry was synced)");
                            return;
                        }

                        // Delete the matched file(s) from the local song library (there can be copies in multiple
                        // subfolders). Only if the file deletion went through is the migration state bumped, so a
                        // failed delete is going to be retried automatically on the next startup.
                        foreach (string songFilePath in filesToDelete)
                        {
                            try
                            {
                                File.Delete(songFilePath);
                            }
                            catch (Exception ex)
                            {
                                if (!File.Exists(songFilePath))
                                    continue; // Another client (e.g. sharing the library via NAS) already deleted this copy

                                // The migration is already on the server, but the library migration state was not bumped,
                                // so this deletion is going to be retried automatically on the next startup.
                                MessageBox.Show("The server registered the deletion, but the file could not be deleted locally:\n" + ex.Message);
                                return;
                            }
                        }

                        // Remove the song from the playlist (only the actually deleted files)
                        foreach (string songFilePath in filesToDelete)
                            SongManager.Playlist.RemoveAll(x => x == songFilePath);

                        // Remove the local database entry (the server already removed its copy of the row).
                        // The history entries referencing the deleted song are removed as well (the server does
                        // that via its database cascade), and so are queued unsynced requests that belonged to
                        // the deleted song (votes, volume, ...), since they can never succeed meaningfully anymore.
                        using (var songDbContext = new SongDbContext())
                        {
                            var songIdsToRemove = new[] { targetSongId };
                            var upvotedSongsToRemove = songDbContext.UpvotedSongs.Where(x => x.SongId == targetSongId).ToArray();
                            var historyToRemove = songDbContext.SongHistoryEntries.Where(h => h.SongId != null && songIdsToRemove.Contains(h.SongId.Value)).ToArray();
                            var queuedToRemove = songDbContext.NotYetSyncedData.Where(n => n.BelongedToSongId != null && songIdsToRemove.Contains(n.BelongedToSongId.Value)).ToArray();

                            songDbContext.SongHistoryEntries.RemoveRange(historyToRemove);
                            songDbContext.UpvotedSongs.RemoveRange(upvotedSongsToRemove);
                            songDbContext.NotYetSyncedData.RemoveRange(queuedToRemove);
                            try
                            {
                                songDbContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                // The file is already deleted, the local database heals itself on the next pull.
                                MessageBox.Show("The file was deleted, but the local database could not be updated:\n" + ex.Message);
                                return;
                            }
                        }

                        SyncManager.WriteSongLibraryMigrationState(Config.Data.MusicPath, createdMigration.UserId, createdMigration.MigrationNumber);

                        // If the library turned out to be registered for a different account, warn about it
                        var ownerWarning = SyncManager.TakeSongLibraryOwnerWarning();
                        if (ownerWarning != null)
                            MessageBox.Show(ownerWarning, "Song Library");

                        SongManager.CreateSongChoosingList();
                        bRefresh_Click(null, EventArgs.Empty);

                        MessageBox.Show("Successfully deleted \"" + songName + "\" from the database and the song libraries!" + (skippedFiles > 0
                            ? "\n\nNote: " + skippedFiles + " file(s) with the old name were left alone, since their album/artist metadata did not match the deleted song entries."
                            : ""));
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!", e.ToString()); }
                })));

                currentMouseOverRow = e.RowIndex;
                m.Show(dataGridView1, new Point(e.X + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, true).X, e.Y + dataGridView1.GetRowDisplayRectangle(e.RowIndex, true).Y));
            }

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                currentMouseOverRow = e.RowIndex;
        }

        // Other Events
        static void RollbackFileRenames(List<(string OldPath, string NewPath)> movedFiles)
        {
            for (int i = movedFiles.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (File.Exists(movedFiles[i].NewPath) && !File.Exists(movedFiles[i].OldPath))
                        File.Move(movedFiles[i].NewPath, movedFiles[i].OldPath);
                }
                catch { }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                bSearch_Click(this, EventArgs.Empty);
        }
        private void Statistics_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsClosed = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerTicks++;
            if (timerTicks == 2 && Math.Abs(MousePos.X - MouseDrag.X + MousePos.Y - MouseDrag.Y) < 15)
            {
                string path = SongManager.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                string[] files = new string[1]; files[0] = path;
                dataGridView1.DoDragDrop(new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy);
                timer1.Enabled = false;
            }
        }

        public void toSong(string Song)
        {            int index = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                if ((string)dataGridView1.Rows[i].Cells[0].Value == Song)
                {
                    index = i;
                    break;
                }

            dataGridView1.FirstDisplayedScrollingRowIndex = index;
        }
        public void filterFor(string filter)
        {
            List<DataGridViewRow> Rows = new List<DataGridViewRow>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString().Contains(filter))
                    Rows.Add(dataGridView1.Rows[i]);
            }
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.AddRange(Rows.ToArray());
        }
    }
}
