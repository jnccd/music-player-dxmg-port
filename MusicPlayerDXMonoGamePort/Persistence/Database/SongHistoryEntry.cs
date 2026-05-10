using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database
{
    public class SongHistoryEntry(string SongName, float ScoreChange, DateTimeOffset Date, string UserId = "")
    {
        public string UserId { get; set; } = UserId;
        public string SongName { get; set; } = SongName;
        public DateTimeOffset Date { get; set; } = Date;
        public float ScoreChange { get; set; } = ScoreChange;
    }
}
