using CryptoBank.Features.News.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.Configuration.News;

public sealed class NewsConfiguration : IEntityTypeConfiguration<New>
{
    public void Configure(EntityTypeBuilder<New> builder)
    {
        builder.ToTable("news");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("title");

        builder.Property(e => e.Date)
            .HasColumnName("date");

        builder.Property(e => e.Author)
            .HasMaxLength(20)
            .HasColumnName("author");

        builder.Property(e => e.Description)
            .HasMaxLength(2048)
            .HasColumnName("description");
    }
}
