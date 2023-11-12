using CryptoBank.Features.Accounts.Domain;
using CryptoBank.Features.Authenticate.Domain;
using CryptoBank.Features.Deposits.Domain;
using CryptoBank.Features.Management.Domain;
using CryptoBank.Features.News.Domain;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database;

public class Context : DbContext
{
    public DbSet<New> News { get; set; }
    public DbSet<User> Users { get; set; }  
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Xpub> Xpubs { get; set; }
    public DbSet<DepositAddress> DepositAddresses { get; set; }
    public DbSet<Variable> Variables { get; set; }

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
