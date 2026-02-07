using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class StoreUser : Entity<Guid>
    {
        public Guid StoreId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Store Store { get; set; }
        public ApplicationUser User { get; set; }
    }

}
