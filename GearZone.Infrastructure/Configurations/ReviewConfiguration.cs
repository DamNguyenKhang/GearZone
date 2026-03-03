using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Rating)
                   .IsRequired();

            builder.Property(x => x.Title)
                   .HasMaxLength(200);

            builder.Property(x => x.Comment)
                   .HasMaxLength(2000);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Reviews)
                   .HasForeignKey(x => x.ProductId);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Reviews)
                   .HasForeignKey(x => x.UserId);

            builder.HasIndex(x => new { x.ProductId, x.UserId })
                   .IsUnique();
        }
    }
}
