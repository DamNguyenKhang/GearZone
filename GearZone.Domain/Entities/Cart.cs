using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Cart : Entity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public Guid? StoreId { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Store? Store { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

}
