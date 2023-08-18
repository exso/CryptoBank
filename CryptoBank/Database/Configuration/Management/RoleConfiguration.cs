using CryptoBank.Features.Management.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Management;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(400)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(400)
            .HasColumnName("description");
    }
}
