using Models = WebApiTesting.Shared.Models;
using Microsoft.EntityFrameworkCore;
using WebApiTesting.Shared.Models;

namespace WebApiTesting.DAL.Data
{
    public class IndicatorsContext : DbContext
    {
        public static string? ConnectionString;

        public DbSet<Value> values { get; set; }
        public DbSet<Result> results { get; set; }
        public DbSet<Models.File> files { get; set; }
        public IndicatorsContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public IndicatorsContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
