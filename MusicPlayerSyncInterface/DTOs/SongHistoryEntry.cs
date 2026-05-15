using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MusicPlayerSyncInterface.DTOs;

public class SongHistoryEntry(Guid? SongId, float ScoreChange, DateTimeOffset Date, string UserId = "")
{
    public string UserId { get; set; } = UserId;
    public Guid? SongId { get; set; } = SongId;
    [ForeignKey("SongId"), JsonIgnore]
    public UpvotedSong? UpvotedSong { get; set; } = null;
    public DateTimeOffset Date { get; set; } = Date;
    public float ScoreChange { get; set; } = ScoreChange;
}
