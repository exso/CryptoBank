using CryptoBank.Features.Authenticate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Authenticate;

public sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens");
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

        builder.Property(e => e.Revoked)
            .HasColumnName("revoked");

        builder.Property(e => e.ReplacedByToken)
            .HasMaxLength(256)
            .HasColumnName("replaced_by_token");

        builder.Property(e => e.ReasonRevoked)
            .HasMaxLength(20)
            .HasColumnName("reason_revoked");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            ;
    }
}
