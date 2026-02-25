using GearZone.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class StoreUserConfiguration : IEntityTypeConfiguration<StoreUser>
    {
        public void Configure(EntityTypeBuilder<StoreUser> builder)
        {
            builder.ToTable("StoreUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role).HasMaxLength(20).IsRequired();

            builder.HasIndex(x => new { x.StoreId, x.UserId }).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.StoreId);

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.StoreUsers)
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.StoreUsers)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
