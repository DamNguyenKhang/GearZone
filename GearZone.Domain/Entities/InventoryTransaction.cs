using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class InventoryTransaction : Entity<Guid>
    {
        public Guid VariantId { get; set; }
        public string Type { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }

        // Navigation
        public ProductVariant Variant { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
    }

}
