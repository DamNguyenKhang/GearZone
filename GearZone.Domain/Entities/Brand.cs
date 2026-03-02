using System;
using System.Collections.Generic;

namespace GearZone.Domain.Entities
{
    public class Brand : Entity<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsApproved { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
