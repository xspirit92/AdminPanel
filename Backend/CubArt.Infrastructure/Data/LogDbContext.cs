using CubArt.Domain.Entities;
using CubArt.Domain.Events;
using CubArt.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Data
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Игнорируем DomainEvent и другие не-entity классы
            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfiguration(new SystemLogConfiguration());
        }
    }

}
