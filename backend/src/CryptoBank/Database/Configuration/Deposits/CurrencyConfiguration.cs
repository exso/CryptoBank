using CryptoBank.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Deposits;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(3)
            .HasColumnName("code");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("name");
    }
}