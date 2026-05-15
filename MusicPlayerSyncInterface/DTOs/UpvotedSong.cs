using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MusicPlayerSyncInterface.DTOs;

public class UpvotedSong(string Name, float Score, int Streak, int TotalLikes, int TotalDislikes, DateTimeOffset? DateAdded, float Volume, string Artist = "", string Album = "", string UserId = "")
{
    public Guid SongId { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = UserId;
    /// <summary>
    /// This refers simply to the filename of the song including the extension, so like "Cool Artist - Nice Song.mp3".
    /// </summary>
    public string Name { get; set; } = Name;
    public string Artist { get; set; } = Artist;
    public string Album { get; set; } = Album;

    public float Score { get; set; } = Score;
    public int Streak { get; set; } = Streak;
    public int TotalLikes { get; set; } = TotalLikes;
    public int TotalDislikes { get; set; } = TotalDislikes;
    public DateTimeOffset? DateAdded { get; set; } = DateAdded;
    public float Volume { get; set; } = Volume;

    [NotMapped, JsonIgnore]
    public string? Path;
}
