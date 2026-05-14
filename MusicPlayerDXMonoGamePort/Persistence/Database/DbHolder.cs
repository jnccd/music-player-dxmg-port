using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;
using Persistence.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerDXMonoGamePort.Persistence.Database
{
    public static class DbHolder
    {
        static readonly object lockject = new();
        public static SongDbContext DbContext
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
        private static SongDbContext context = new();

        public static void SaveChanges()
        {
            lock (lockject)
            {
                context.SaveChanges();
            }
        }
    }
}
