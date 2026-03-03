using System;

namespace GearZone.Domain.Entities
{
    public class Review : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public int Rating { get; set; } // 1-5
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
