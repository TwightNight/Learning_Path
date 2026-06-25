using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.ToTable(nameof(Chapter));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.ChapterNumber)
                .HasPrecision(8, 2);

            builder.Property(x => x.Content)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.WordCount)
                .HasDefaultValue(0);

            builder.Property(x => x.ViewCount)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Chapters)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Volume)
                .WithMany(x => x.Chapters)
                .HasForeignKey(x => x.VolumeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.HasIndex(x => x.NovelId);

            builder.HasIndex(x => x.VolumeId);

            builder.HasIndex(x => new
            {
                x.NovelId,
                x.VolumeId,
                x.ChapterNumber
            })
            .IsUnique();
        }
    }
}
