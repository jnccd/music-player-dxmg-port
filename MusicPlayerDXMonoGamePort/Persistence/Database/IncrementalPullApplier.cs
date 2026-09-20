using System;
using System.Collections.Generic;
using System.Linq;
using MusicPlayerSyncInterface.DTOs;
using MusicPlayerSyncInterface.DTOs.Composites;

namespace MusicPlayerDXMonoGamePort.Persistence.Database
{
    /// <summary>
    /// Applies an incremental /sync/pull (see SyncManager.Pull): the songs are always the complete account
    /// list and are reconciled with the local rows (updated, added, removed), while the history is only the
    /// delta since the client's cursor and is APPENDED - deduplicated by its primary key - instead of
    /// replacing the local history. This keeps the pull payload (and therefore the request that nginx has
    /// to accept) bounded even though the history grows with every played song.
    /// History of songs that no longer exist locally is deliberately kept: an orphan is harmless (no code
    /// path requires the song of a history entry to exist) and the server never reports the deletions, so
    /// the caller only fails a pull when entries are MISSING, never when there are extra ones.
    /// </summary>
    public static class IncrementalPullApplier
    {
        /// <summary>
        /// Number of history entries this client holds for one account. Used to tell the server how much
        /// the client knows (incremental pull bootstrap) and to verify afterwards that nothing is missing.
        /// </summary>
        public static int CountLocalHistory(SongDbContext songDbContext, string userId) =>
            string.IsNullOrEmpty(userId) ? 0 : songDbContext.SongHistoryEntries.Count(h => h.UserId == userId);

        /// <summary>
        /// Applies the incremental pull data. Returns how many history entries were newly appended and how
        /// many duplicate song rows were merged away.
        /// </summary>
        public static (int NewHistoryEntries, int MergedDuplicates) Apply(SongDbContext songDbContext, SyncPullResponse pulledData, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("An incremental pull needs the account id.", nameof(userId));

            UpvotedSong[] pulledSongs = pulledData.Songs ?? new UpvotedSong[0];
            SongHistoryEntry[] pulledHistory = pulledData.HistoryEntries ?? new SongHistoryEntry[0];
            var pulledSongIds = new HashSet<Guid>(pulledSongs.Select(song => song.SongId));

            // 1. Remove the account's local rows the server no longer has (deleted songs, or the loser
            //    rows of a duplicate merge). Local-only rows (UserId "", i.e. not synced yet) stay.
            var songsToRemove = songDbContext.UpvotedSongs
                .Where(song => song.UserId == userId)
                .ToArray()
                .Where(song => !pulledSongIds.Contains(song.SongId))
                .ToArray();
            if (songsToRemove.Length > 0)
            {
                songDbContext.UpvotedSongs.RemoveRange(songsToRemove);
                songDbContext.SaveChanges();
            }

            // 2. Upsert the pulled songs: existing local rows are updated in place (adding a second
            //    instance with the same key would make EF throw), missing ones are inserted.
            var localSongsBySongId = songDbContext.UpvotedSongs
                .Where(song => song.UserId == userId || song.UserId == "")
                .ToArray()
                .GroupBy(song => song.SongId)
                .ToDictionary(group => group.Key, group => group.First());
            foreach (UpvotedSong pulledSong in pulledSongs)
            {
                UpvotedSong localSong;
                if (localSongsBySongId.TryGetValue(pulledSong.SongId, out localSong))
                {
                    localSong.UserId = pulledSong.UserId;
                    localSong.Name = pulledSong.Name;
                    localSong.Artist = pulledSong.Artist;
                    localSong.Album = pulledSong.Album;
                    localSong.Score = pulledSong.Score;
                    localSong.Streak = pulledSong.Streak;
                    localSong.TotalLikes = pulledSong.TotalLikes;
                    localSong.TotalDislikes = pulledSong.TotalDislikes;
                    localSong.DateAdded = pulledSong.DateAdded;
                    localSong.Volume = pulledSong.Volume;
                    localSong.Path = pulledSong.Path;
                }
                else
                {
                    songDbContext.UpvotedSongs.Add(pulledSong);
                }
            }

            // Also make sure the account row itself exists (the server sends it with every pull).
            if (pulledData.User != null && !songDbContext.Users.Any(user => user.UserId == pulledData.User.UserId))
                songDbContext.Users.Add(pulledData.User);
            songDbContext.SaveChanges();

            // 3. Append the history delta. Entries that are already present (the cursor is inclusive, and
            //    the bootstrap verification tail overlaps what the client has) are skipped by their primary
            //    key (user + song + date). Entries of unknown songs are ignored defensively.
            var knownHistoryKeys = new HashSet<Tuple<Guid, DateTimeOffset>>(songDbContext.SongHistoryEntries
                .Where(entry => entry.UserId == userId && entry.SongId != null)
                .ToArray()
                .Select(entry => Tuple.Create(entry.SongId.Value, entry.Date)));
            int newHistoryEntries = 0;
            foreach (SongHistoryEntry entry in pulledHistory)
            {
                if (entry.SongId == null || !pulledSongIds.Contains(entry.SongId.Value))
                    continue;
                entry.UserId = userId;
                if (knownHistoryKeys.Add(Tuple.Create(entry.SongId.Value, entry.Date)))
                {
                    songDbContext.SongHistoryEntries.Add(entry);
                    newHistoryEntries++;
                }
            }
            if (newHistoryEntries > 0)
                songDbContext.SaveChanges();

            // 4. Duplicates can still arrive from a server that was not healed yet: merge them like after
            //    a full pull, so statistics and the song choosing only ever see one row per song.
            int mergedDuplicates = UpvotedSongMerger.MergeDuplicateUpvotedSongs(songDbContext);

            return (newHistoryEntries, mergedDuplicates);
        }
    }
}
