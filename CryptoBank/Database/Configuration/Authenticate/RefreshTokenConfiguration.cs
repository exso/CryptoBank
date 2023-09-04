using CryptoBank.Features.Authenticate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Authenticate;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.Token)
            .HasMaxLength(256)
            .HasColumnName("token");

        builder.Property(e => e.Expires)
            .HasColumnName("expires");

        builder.Property(e => e.Created)
            .HasColumnName("created");

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(20)
            .HasColumnName("created_by_ip");

        builder.Property(e => e.Revoked)
            .HasColumnName("revoked");

        builder.Property(e => e.RevokedByIp)
            .HasMaxLength(20)
            .HasColumnName("revoked_by_ip");

        builder.Property(e => e.ReplacedByToken)
            .HasMaxLength(256)
            .HasColumnName("replaced_by_token");

        builder.Property(e => e.ReasonRevoked)
            .HasMaxLength(20)
            .HasColumnName("reason_revoked");

        builder.Property(e => e.IsExpired).HasColumnName("is_expired");

        builder.Property(e => e.IsRevoked).HasColumnName("is_revoked");

        builder.Property(e => e.IsActive).HasColumnName("is_active");

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            ;
    }
}
