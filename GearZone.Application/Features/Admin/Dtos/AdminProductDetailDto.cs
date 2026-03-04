using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public AdminCategoryInfoDto Category { get; set; } = null!;
        public AdminBrandInfoDto Brand { get; set; } = null!;
        public AdminStoreInfoDto Store { get; set; } = null!;

        public List<string> Images { get; set; } = new List<string>();
        public Dictionary<string, string> Specs { get; set; } = new Dictionary<string, string>();
        public List<AdminProductVariantDto> Variants { get; set; } = new List<AdminProductVariantDto>();
    }

    public class AdminCategoryInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AdminBrandInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AdminStoreInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VendorId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
