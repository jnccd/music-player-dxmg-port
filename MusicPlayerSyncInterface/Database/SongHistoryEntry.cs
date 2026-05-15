using System.ComponentModel.DataAnnotations.Schema;

namespace MusicPlayerSyncInterface.Database;

public class SongHistoryEntry(Guid? SongId, float ScoreChange, DateTimeOffset Date, string UserId = "")
{
    public string UserId { get; set; } = UserId;
    public Guid? SongId { get; set; } = SongId;
    [ForeignKey("SongId")]
    public UpvotedSong? UpvotedSong { get; set; } = null;
    public DateTimeOffset Date { get; set; } = Date;
    public float ScoreChange { get; set; } = ScoreChange;
}
