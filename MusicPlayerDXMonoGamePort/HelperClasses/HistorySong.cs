using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerDXMonoGamePort
{
    public class HistorySong
    {
        public HistorySong(string Name, float Change, long Date)
        {
            this.Name = Name;
            this.Change = Change;
            this.Date = Date;
        }

        public string Name;
        public float Change;
        public long Date;
    }
}
