using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MusicPlayerDXMonoGamePort.Persistence.Database;

namespace Persistence.Database
{
    public class SongHistoryEntry(Guid? SongId, float ScoreChange, DateTimeOffset Date, string UserId = "")
    {
        public string UserId { get; set; } = UserId;
        public Guid? SongId { get; set; } = SongId;
        [ForeignKey("SongId")]
        public UpvotedSong? UpvotedSong { get; set; } = null;
        public string SongName { get; set; } = DbHolder.DbContext.UpvotedSongs.FirstOrDefault(s => s.SongId == SongId)?.Name ?? "";
        public DateTimeOffset Date { get; set; } = Date;
        public float ScoreChange { get; set; } = ScoreChange;
    }
}
