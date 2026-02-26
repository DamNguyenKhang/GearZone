using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class Store : Entity<Guid>
    {
        public string OwnerUserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }

        public string TaxCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;

        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;

        public StoreStatus Status { get; set; }
        public string? RejectReason { get; set; }
        public string? LockReason { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
 
        public ApplicationUser OwnerUser { get; set; } = null!;
        public ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
