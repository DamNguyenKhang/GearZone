using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class ProductVariant : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Product Product { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }

}
