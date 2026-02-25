using GearZone.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId).IsUnique();
            builder.HasIndex(x => x.StoreId);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Carts)
                   .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Store)
                   .WithMany()
                   .HasForeignKey(x => x.StoreId);
        }
    }
}
