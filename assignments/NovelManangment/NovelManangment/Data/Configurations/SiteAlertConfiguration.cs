using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovelManangment.Models;

namespace NovelManangment.Data.Configurations
{
    public class SiteAlertConfiguration : IEntityTypeConfiguration<SiteAlert>
    {
        public void Configure(EntityTypeBuilder<SiteAlert> builder)
        {
            builder.ToTable(nameof(SiteAlert));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue(AlertType.Info);
        }
    }
}
