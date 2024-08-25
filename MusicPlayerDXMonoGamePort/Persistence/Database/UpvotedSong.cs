using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database
{
    public class UpvotedSong(string Name, float Score, int Streak, int TotalLikes, int TotalDislikes, long AddingDates, float Volume)
    {
        [Key]
        public string Name { get; set; } = Name;
        public float Score { get; set; } = Score;
        public int Streak { get; set; } = Streak;
        public int TotalLikes { get; set; } = TotalLikes;
        public int TotalDislikes { get; set; } = TotalDislikes;
        public long AddingDates { get; set; } = AddingDates;
        public float Volume { get; set; } = Volume;

        [NotMapped]
        public string Path; // Only used in ExportChooser.cs
    }
}
