using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NewStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new { x.OrderId, x.ChangedAt });
            builder.HasIndex(x => x.ChangedByUserId);

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.StatusHistories)
                   .HasForeignKey(x => x.OrderId);
        }
    }
}
