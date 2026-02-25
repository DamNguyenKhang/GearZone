using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Entities
{
    public class Order : Entity<Guid>
    {
        public string OrderCode { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Guid StoreId { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal CommissionRateSnapshot { get; set; }
        public decimal CommissionAmount { get; set; }

        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ShippingProvider { get; set; }
        public string? TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
