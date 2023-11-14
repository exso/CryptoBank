using CryptoBank.Features.Deposits.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoBank.Database.Configuration.Deposits;

public sealed class VariableConfiguration : IEntityTypeConfiguration<Variable>
{
    public void Configure(EntityTypeBuilder<Variable> builder)
    {
        builder.ToTable("variables");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("key");

        builder.Property(e => e.Value)
            .IsRequired()
            .HasColumnName("value");
    }
}
