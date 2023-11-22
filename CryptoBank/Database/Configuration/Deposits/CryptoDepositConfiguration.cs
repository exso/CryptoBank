using CryptoBank.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Deposits;

public sealed class CryptoDepositConfiguration : IEntityTypeConfiguration<CryptoDeposit>
{
    public void Configure(EntityTypeBuilder<CryptoDeposit> builder)
    {
        builder.ToTable("crypto_deposits");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.AddressId)
            .IsRequired()
            .HasColumnName("address_id");

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(20, 2)
            .HasColumnName("amount");

        builder.Property(e => e.CurrencyId)
            .IsRequired()
            .HasColumnName("currency_id");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(e => e.TxId)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("tx_id");

        builder.Property(e => e.Confirmations)
            .IsRequired()
            .HasColumnName("confirmations");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasColumnName("status");

        builder.HasOne(x => x.User)
            .WithMany(x => x.CryptoDeposits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Address)
            .WithMany(x => x.CryptoDeposits)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.CryptoDeposits)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
