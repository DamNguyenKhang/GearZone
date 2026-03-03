using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Products
{
    public class DetailModel : PageModel
    {
        public ProductDetailViewModel Product { get; set; }

        public void OnGet(Guid id)
        {
            // If id is empty, use a dummy one for the prototype
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
            }

            // Mock Data for Prototype
            Product = new ProductDetailViewModel
            {
                Id = id,
                Name = "Keychron K8 Pro Wireless Mechanical Keyboard",
                Sku = "KEY-K8P-RGB-BROWN",
                Description = "<p class=\"mb-4\">The Keychron K8 Pro is the world's first out-of-the-box QMK/VIA wireless mechanical keyboard paving the way for a new era for mechanical keyboards.</p><p class=\"mb-4\">Anyone can master any keyboard keys or macro commands through VIA on a wireless keyboard. Together with our signature features and upgraded typing sound, the K8 Pro is advancing the typing experience to an entirely new level with endless possibilities.</p><ul class=\"list-disc pl-5 space-y-1 mb-4\"><li>Wireless and Wired modes</li><li>Hot-swappable switches</li><li>Mac & Windows support</li><li>Sound Absorbing Foam</li></ul>",
                Status = "Pending", // Draft, Pending, Active, Suspended, Rejected
                BasePrice = 2500000,
                Stock = 145,
                CommissionRate = 10.0m, // 10%
                CreatedAt = DateTime.Now.AddMonths(-1),
                UpdatedAt = DateTime.Now,
                
                Category = new CategoryInfo { Id = 1, Name = "Mechanical Keyboards" },
                Brand = new BrandInfo { Id = 1, Name = "Keychron" },
                Store = new StoreInfo 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Keychron Official Store", 
                    VendorId = "VND-4029",
                    JoinedAt = DateTime.Now.AddYears(-2),
                    AvatarUrl = "https://placehold.co/150x150/232323/ffffff?text=Keychron"
                },

                Images = new List<string>
                {
                    "https://placehold.co/800x600/1E293B/ffffff?text=K8+Pro+Main",
                    "https://placehold.co/800x600/334155/ffffff?text=K8+Pro+Side",
                    "https://placehold.co/800x600/475569/ffffff?text=K8+Pro+Switches",
                    "https://placehold.co/800x600/64748B/ffffff?text=K8+Pro+Accessories"
                },

                Specs = new Dictionary<string, string>
                {
                    { "Switch Type", "Gateron G Pro Brown" },
                    { "Connectivity", "Bluetooth 5.1 / Type-C Wired" },
                    { "Battery", "4000mAh (Up to 240 hrs with backlight off)" },
                    { "Layout", "87 Keys (Tenkeyless)" },
                    { "Material", "Aluminum Frame + ABS Bottom" }
                },

                Variants = new List<ProductVariantViewModel>
                {
                    new ProductVariantViewModel { Sku = "KEY-K8P-RGB-BROWN", Name = "RGB Backlight - Brown Switch", Price = 2500000, Stock = 50 },
                    new ProductVariantViewModel { Sku = "KEY-K8P-RGB-RED", Name = "RGB Backlight - Red Switch", Price = 2500000, Stock = 75 },
                    new ProductVariantViewModel { Sku = "KEY-K8P-RGB-BLUE", Name = "RGB Backlight - Blue Switch", Price = 2500000, Stock = 20 }
                }
            };
        }
    }

    public class ProductDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public CategoryInfo Category { get; set; }
        public BrandInfo Brand { get; set; }
        public StoreInfo Store { get; set; }

        public List<string> Images { get; set; }
        public Dictionary<string, string> Specs { get; set; }
        public List<ProductVariantViewModel> Variants { get; set; }
    }

    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BrandInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StoreInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string VendorId { get; set; }
        public DateTime JoinedAt { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class ProductVariantViewModel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
