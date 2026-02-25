using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Entities
{
    public class Store : Entity<Guid>
    {
        public Guid BusinessId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? LockReason { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
 
        // Navigation
        public Business Business { get; set; } = null!;
        public ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
