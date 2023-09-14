using CryptoBank.Features.Accounts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Accounts;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(x => x.Number);

        builder.Property(e => e.Number)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnName("number");

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("currency");

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(20, 2)
            .HasColumnName("amount");

        builder.Property(e => e.DateOfOpening)
            .HasColumnName("date_of_opening");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserAccounts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
