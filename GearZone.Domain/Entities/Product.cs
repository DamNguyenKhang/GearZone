using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        public Guid StoreId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string SpecsJson { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Store Store { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductVariant> Variants { get; set; }
    }

}
