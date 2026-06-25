using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class NovelRatingConfiguration : IEntityTypeConfiguration<NovelRating>
    {
        public void Configure(EntityTypeBuilder<NovelRating> builder)
        {
            builder.ToTable(nameof(NovelRating));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Score)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.User)
                .WithMany(x => x.Ratings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Ratings)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 user chỉ rate 1 novel 1 lần
            builder.HasIndex(x => new { x.UserId, x.NovelId })
                .IsUnique();
        }
    }
}
