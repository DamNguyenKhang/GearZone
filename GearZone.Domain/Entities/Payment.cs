using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Payment : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public string Method { get; set; }
        public string Provider { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Order Order { get; set; }
    }
}
