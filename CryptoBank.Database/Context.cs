using CryptoBank.Objects.News;

using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<New> News { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}
