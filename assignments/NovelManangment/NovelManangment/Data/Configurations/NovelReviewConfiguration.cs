using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class NovelReviewConfiguration : IEntityTypeConfiguration<NovelReview>
    {
        public void Configure(EntityTypeBuilder<NovelReview> builder)
        {
            builder.ToTable(nameof(NovelReview));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(x => x.Note)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Moderator)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ModeratorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.NovelId);
            builder.HasIndex(x => x.ModeratorId);
        }
    }
}
