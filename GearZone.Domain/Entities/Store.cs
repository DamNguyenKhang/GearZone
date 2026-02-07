using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Store : Entity<Guid>
    {
        public Guid BusinessId { get; set; }
        public string StoreName { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Status { get; set; }
        public string LockReason { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Business Business { get; set; }
        public ICollection<StoreUser> StoreUsers { get; set; }
        public ICollection<Product> Products { get; set; }
    }

}
