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

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (Environment.GetEnvironmentVariable("DB_PROVIDER") == "postgres")
            {
                options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_DB_ACCESS"));
            }
            else if (Environment.GetEnvironmentVariable("DB_PROVIDER") == "sqlite" || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PROVIDER")))
            {
                var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
                var sqlitePath = (Environment.GetEnvironmentVariable("MUSIC_PLAYER_SQLITE_DB_PATH") ?? exePath) + "song.db";

                options.UseSqlite($"Data Source={sqlitePath}");
            }
            else
            {
                throw new InvalidOperationException("No valid DB_PROVIDER environment variable set. Use 'sqlite' or 'postgres'.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UpvotedSong>()
                .HasKey(s => s.SongId);
            modelBuilder.Entity<UpvotedSong>()
                .HasIndex(s => new { s.UserId, s.Name, s.Artist, s.Album })
                .IsUnique();

            modelBuilder.Entity<SongHistoryEntry>()
                .HasKey(s => new { s.UserId, s.SongId, s.Date });
            modelBuilder.Entity<SongHistoryEntry>()
                .HasOne(s => s.UpvotedSong)
                .WithMany() // No navigation property back to SongHistoryEntry
                .HasForeignKey(s => s.SongId);
        }
    }
}
