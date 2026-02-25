using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Entities
{
    public class OrderStatusHistory : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string ChangedByUserId { get; set; } = string.Empty;
        public string? Note { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public ApplicationUser ChangedByUser { get; set; } = null!;
    }
}
