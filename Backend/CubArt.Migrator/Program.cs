#nullable enable
using CubArt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace CubArt.Migrator
{
    public class Program : IDesignTimeDbContextFactory<AppDbContext>
    {
        static async Task Main(string[] args)
        {
            Program program = new Program();

            await using (AppDbContext dbContext = program.CreateDbContext())
            {
                await dbContext.Database.MigrateAsync();
            }

            Console.WriteLine("Migrate successfully! 😎😎😎");
        }

        public AppDbContext CreateDbContext(string[]? args = null)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            var configuration = configurationBuilder.Build();
            var connectionString = configuration.GetConnectionString("DBConnectionString");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString, x => x.MigrationsAssembly(typeof(Program).Namespace));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}