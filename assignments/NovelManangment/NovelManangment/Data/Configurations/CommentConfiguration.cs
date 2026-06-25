using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable(nameof(Comment));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.User)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Chapter)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
