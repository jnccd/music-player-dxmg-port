using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database
{
    public class SongDbContext : DbContext
    {
        public DbSet<UpvotedSong> UpvotedSongs { get; set; }
        public DbSet<SongHistoryEntry> SongHistoryEntries { get; set; }

        private static readonly string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
        private static readonly string dbPath = (Environment.GetEnvironmentVariable("MUSIC_PLAYER_SQLITE_DB_PATH") ?? exePath) + "song.db";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={dbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UpvotedSong>()
                .HasKey(s => s.SongId);
            modelBuilder.Entity<UpvotedSong>()
                .HasIndex(s => new { s.UserId, s.Name, s.Artist, s.Album })
                .IsUnique();

            modelBuilder.Entity<SongHistoryEntry>()
                .HasKey(s => new { s.UserId, s.SongName, s.Date });
        }
    }
}
