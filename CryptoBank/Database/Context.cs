using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.News.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database;

public class Context : DbContext
{
    public DbSet<New> News { get; set; }
    public DbSet<User> Users { get; set; }

    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
