using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;
using System.Reflection.Emit;

namespace NovelManangment.Data.Configurations
{
    public class NovelConfiguration : IEntityTypeConfiguration<Novel>
    {
        public void Configure(EntityTypeBuilder<Novel> builder)
        {
            builder.ToTable(nameof(Novel));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AlternativeTitle)
                .HasMaxLength(255);

            builder.Property(x => x.Slug)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.AuthorName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.ArtistName)
                .HasMaxLength(255);

            builder.Property(x => x.CoverUrl) 
                .HasMaxLength(400);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsUnicode(false);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.Language)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PublishStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue(PublishStatus.Draft);

            builder.HasOne(x => x.Publisher)
                .WithMany()
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(n => !n.IsDeleted);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.HasIndex(x => x.Title);

            builder.HasMany(x => x.Genres)
                .WithMany(x => x.Novels);
        }
    }
}
