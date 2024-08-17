using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Kyyla.Model
{
    public class LockEventDbContext : DbContext
    {
        public DbSet<LockEvent> LockEvents => Set<LockEvent>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataFolder = Path.Combine(appDataPath, "kyyla");
            var databasePath = Path.Combine(appDataFolder, "locktimes.db");
            options.UseSqlite($"Data Source={databasePath}");
        }
    }
}
