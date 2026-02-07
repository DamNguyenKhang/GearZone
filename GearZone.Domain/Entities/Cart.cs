using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Cart : Entity<Guid>
    {
        public string UserId { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public Store Store { get; set; }
        public ICollection<CartItem> Items { get; set; }
    }

}
