using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Entities
{
    public class Payment : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
