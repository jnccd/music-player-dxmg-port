using Persistence;
using Persistence.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerDXMonoGamePort.Persistence.Database
{
    public static class DbHolder
    {
        static readonly object lockject = new();
        public static UpvotedSongDbContext DbContext
        {
            get
            {
                lock (lockject)
                {
                    return context;
                }
            }
            set
            {
                context = value;
            }
        }
        private static UpvotedSongDbContext context = new();
    }
}
