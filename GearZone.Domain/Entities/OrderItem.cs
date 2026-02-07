using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class OrderItem : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public Guid VariantId { get; set; }

        public string ProductNameSnapshot { get; set; }
        public string VariantNameSnapshot { get; set; }
        public string SkuSnapshot { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation
        public Order Order { get; set; }
        public ProductVariant Variant { get; set; }
    }

}
