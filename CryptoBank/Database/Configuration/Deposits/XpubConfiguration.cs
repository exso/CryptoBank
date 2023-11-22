using CryptoBank.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Deposits;

public sealed class XpubConfiguration : IEntityTypeConfiguration<Xpub>
{
    public void Configure(EntityTypeBuilder<Xpub> builder)
    {
        builder.ToTable("xpubs");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.CurrencyId)
            .IsRequired()
            .HasColumnName("currency_id");

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("value");

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Xpubs)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
