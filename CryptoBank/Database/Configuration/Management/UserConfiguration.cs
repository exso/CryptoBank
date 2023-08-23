using CryptoBank.Features.Management.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Management
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityColumn();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("email");

            builder.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("password");

            builder.Property(e => e.DateOfBirth)
                .HasColumnName("date_of_birth");

            builder.Property(e => e.DateOfRegistration)
                .HasColumnName("date_of_registration");
        }
    }
}
