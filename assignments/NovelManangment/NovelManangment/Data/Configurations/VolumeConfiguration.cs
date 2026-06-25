using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class VolumeConfiguration : IEntityTypeConfiguration<Volume>
    {
        public void Configure(EntityTypeBuilder<Volume> builder)
        {
            builder.ToTable(nameof(Volume));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.PdfUrl)
                .HasMaxLength(500);

            builder.Property(x => x.CoverUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Volumes)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.NovelId);

            builder.HasIndex(x => new
            {
                x.NovelId,
                x.VolumeNumber
            })
            .IsUnique();
        }
    }
}
