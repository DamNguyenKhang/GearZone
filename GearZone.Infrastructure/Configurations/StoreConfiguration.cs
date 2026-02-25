using GearZone.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StoreName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.LogoUrl).HasMaxLength(1000);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.LockReason).HasMaxLength(500);

            builder.HasIndex(x => x.Slug).IsUnique();
            builder.HasIndex(x => x.BusinessId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Business)
                   .WithMany(x => x.Stores)
                   .HasForeignKey(x => x.BusinessId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
