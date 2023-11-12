using CryptoBank.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Deposits;

public sealed class DepositAddressConfiguration : IEntityTypeConfiguration<DepositAddress>
{
    public void Configure(EntityTypeBuilder<DepositAddress> builder)
    {
        builder.ToTable("deposit_addresses");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.XpubId)
            .HasColumnName("xpub_id");

        builder.Property(e => e.DerivationIndex)
            .IsRequired()
            .HasColumnName("derivation_index");

        builder.Property(e => e.CryptoAddress)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("crypto_address");

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.DepositAddresses)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.DepositAddresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Xpub)
            .WithMany(x => x.DepositAddresses)
            .HasForeignKey(x => x.XpubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
