using Microsoft.AspNetCore.Identity;

namespace GearZone.Application.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public ICollection<Store> OwnedStores { get; set; } = new List<Store>();
        public ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
  