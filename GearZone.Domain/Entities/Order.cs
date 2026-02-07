using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Order : Entity<Guid>
    {
        public string OrderCode { get; set; }
        public string UserId { get; set; }
        public Guid StoreId { get; set; }
        public string Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal CommissionRateSnapshot { get; set; }
        public decimal CommissionAmount { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingProvider { get; set; }
        public string TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public Store Store { get; set; }
        public ICollection<OrderItem> Items { get; set; }
        public ICollection<OrderStatusHistory> StatusHistories { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }

}
