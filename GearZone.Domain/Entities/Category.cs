using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Entities
{
    public class Category : Entity<int>
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Navigation
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
