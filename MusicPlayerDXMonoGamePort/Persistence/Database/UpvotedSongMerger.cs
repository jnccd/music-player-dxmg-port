using System;
using System.Collections.Generic;
using System.Linq;
using MusicPlayerSyncInterface;
using MusicPlayerSyncInterface.DTOs;
using Newtonsoft.Json.Linq;

namespace MusicPlayerDXMonoGamePort.Persistence.Database;

/// <summary>
/// Heals duplicate UpvotedSong rows in the local dxmg database. The dxmg client treats one song as one
/// FILE NAME (its UI, votes and song choosing all look rows up by name), so duplicate rows of the same
/// name - e.g. leftover server duplicates (a metadata-less row next to a tagged row of the same song,
/// possibly under different user ids when rows predate a login) - make the client behave as if one song
/// existed several times: the "not played yet" pool (Streak == 0) keeps containing the song even after
/// it was disliked (only one of its rows got the downvote), and the recent-replay window is tracked per
/// SongId, so the other row bypasses it.
/// Rows are merged per file name when they can be proven to be the same song: the rows must not
/// CONTRADICT each other (both carrying a tag field with different non-empty values would mean
/// different songs; rows that differ only by empty fields - e.g. a pruned artist - are one song).
/// The canonical row is chosen by SongFileMatching (data-first: the row carrying the score history
/// always survives), its counters stay untouched, its empty metadata fields are filled from the
/// combined tags, and the history of the merged-away rows is moved onto it (delete + re-add under the
/// kept row's key, because EF Core cannot modify key properties in place). Returns how many rows were
/// merged away.
/// </summary>
public static class UpvotedSongMerger
{
    public static int MergeDuplicateUpvotedSongs(SongDbContext songDbContext)
    {
        int mergedAway = 0;

        var groups = songDbContext.UpvotedSongs.ToArray()
            .GroupBy(song => song.Name)
            .Where(group => group.Count() > 1)
            .ToArray();

        foreach (var group in groups)
        {
            // Rows of one song may differ by EMPTY tag fields (e.g. an artist pruned on one client);
            // only CONTRADICTING non-empty values (different artist or album on the same name) mean
            // genuinely different same-named songs.
            if (!SongFileMatching.TryGetCombinedTags(group, out string combinedArtist, out string combinedAlbum))
                continue;

            var (keep, remove) = SongFileMatching.MergeSameSongEntries(group, combinedAlbum, combinedArtist);
            mergedAway += MoveHistoryAndRemoveRows(songDbContext, keep, remove);

            // Fill the empty tag fields of the kept row from the combined metadata when the kept row
            // was the one missing them (it won because it carries the song data) - after the other
            // rows were removed.
            if (SongFileMatching.TryFillMissingTags(keep, combinedAlbum, combinedArtist, out string? artistToSet, out string? albumToSet))
            {
                if (artistToSet != null)
                    keep.Artist = artistToSet;
                if (albumToSet != null)
                    keep.Album = albumToSet;
                songDbContext.SaveChanges();
                Console.WriteLine($"Filled metadata of \"{keep.Name}\" (artist: {keep.Artist}, album: {keep.Album}) onto data-carrying row {keep.SongId}.");
            }
        }

        return mergedAway;
    }

    static int MoveHistoryAndRemoveRows(SongDbContext songDbContext, UpvotedSong keep, UpvotedSong[] remove)
    {
        if (remove.Length == 0)
            return 0;

        // Move the history of the merged-away rows onto the kept row (delete + re-add under the kept
        // row's key). Entries colliding with the kept row's history (same date) are the same listening
        // event recorded twice and are dropped. The kept row's counters stay untouched.
        var keepDates = new HashSet<DateTimeOffset>(songDbContext.SongHistoryEntries
            .Where(entry => entry.UserId == keep.UserId && entry.SongId == keep.SongId)
            .Select(entry => entry.Date));

        int movedHistory = 0;
        int droppedDuplicates = 0;
        foreach (UpvotedSong removed in remove)
        {
            var removedHistory = songDbContext.SongHistoryEntries
                .Where(entry => entry.SongId == removed.SongId)
                .ToArray();
            foreach (SongHistoryEntry entry in removedHistory)
            {
                if (keepDates.Add(entry.Date))
                {
                    songDbContext.SongHistoryEntries.Remove(entry);
                    songDbContext.SongHistoryEntries.Add(new SongHistoryEntry(keep.SongId, entry.ScoreChange, entry.Date, keep.UserId));
                    movedHistory++;
                }
                else
                {
                    songDbContext.SongHistoryEntries.Remove(entry);
                    droppedDuplicates++;
                }
            }
        }

        // The dxmg database keeps its NotYetSyncedData queue across pulls. Requests that still reference
        // a merged-away row (queued uploads, votes, volume changes) must be redirected to the kept row,
        // so a retried upload cannot re-create a duplicate under the dead SongId and queued votes still
        // reach the server once the song is uploaded.
        foreach (UpvotedSong removed in remove)
        {
            var queuedForRemoved = songDbContext.NotYetSyncedData
                .Where(queued => queued.BelongedToSongId == removed.SongId)
                .ToArray();
            foreach (var queued in queuedForRemoved)
            {
                try
                {
                    var body = Newtonsoft.Json.Linq.JObject.Parse(queued.Body);
                    var songIdToken = body["SongId"] ?? body["songId"];
                    if (songIdToken != null && songIdToken.Type == Newtonsoft.Json.Linq.JTokenType.String
                        && Guid.TryParse(songIdToken.Value<string>(), out Guid queuedSongId) && queuedSongId == removed.SongId)
                    {
                        songIdToken.Replace(Newtonsoft.Json.Linq.JToken.FromObject(keep.SongId));
                        queued.Body = body.ToString(Newtonsoft.Json.Formatting.Indented);
                        queued.BelongedToSongId = keep.SongId;
                    }
                }
                catch
                {
                    // Unparseable queue body: leave it alone (it will be handled by the normal retry path).
                }
            }
        }

        // Persist the history moves BEFORE deleting the rows, so no database cascade removes them.
        if (movedHistory > 0 || droppedDuplicates > 0)
            songDbContext.SaveChanges();

        songDbContext.UpvotedSongs.RemoveRange(remove);
        songDbContext.SaveChanges();

        Console.WriteLine($"Healed duplicate rows of \"{keep.Name}\" (artist: {keep.Artist}, album: {keep.Album}, user: {keep.UserId}): kept {keep.SongId}, merged away {remove.Length} row(s); moved {movedHistory} history entr{(movedHistory == 1 ? "y" : "ies")} onto the kept row{(droppedDuplicates > 0 ? $", dropped {droppedDuplicates} duplicate history entr{(droppedDuplicates == 1 ? "y" : "ies")} (same date)" : "")}.");

        return remove.Length;
    }
}
