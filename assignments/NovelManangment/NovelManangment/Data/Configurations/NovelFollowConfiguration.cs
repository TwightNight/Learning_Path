using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class NovelFollowConfiguration : IEntityTypeConfiguration<NovelFollow>
    {
        public void Configure(EntityTypeBuilder<NovelFollow> builder)
        {
            builder.ToTable(nameof(NovelFollow));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(x => x.User)
                .WithMany(x => x.Follows)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Novel)
                .WithMany(x => x.Follows)
                .HasForeignKey(x => x.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new 
            { 
                x.UserId, x.NovelId 
            })
            .IsUnique();
        }
    }
}
