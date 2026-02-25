using GearZone.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            builder.ToTable("Businesses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.TaxCode).HasMaxLength(50);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(256);
            builder.Property(x => x.AddressLine).HasMaxLength(500);
            builder.Property(x => x.Province).HasMaxLength(100);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RejectReason).HasMaxLength(500);

            builder.HasIndex(x => x.OwnerUserId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.OwnerUser)
                   .WithMany(x => x.OwnedBusinesses)
                   .HasForeignKey(x => x.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
