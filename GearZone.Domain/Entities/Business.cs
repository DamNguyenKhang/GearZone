using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Business : Entity<Guid>
    {
        public string OwnerUserId { get; set; }
        public string BusinessName { get; set; }
        public string TaxCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string AddressLine { get; set; }
        public string Province { get; set; }
        public string Status { get; set; }
        public string RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Navigation
        public ApplicationUser OwnerUser { get; set; }
        public ICollection<Store> Stores { get; set; }
    }
}
