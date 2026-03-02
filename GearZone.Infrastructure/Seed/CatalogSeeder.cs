using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Seed
{
    public static class CatalogSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            // 1. Get a user for store owner (Super Admin from identity seeder)
            var owner = await context.Users.FirstOrDefaultAsync();
            if (owner == null) return;

            // 2. Create Dummy Store
            var store = new Store
            {
                Id = Guid.NewGuid(),
                OwnerUserId = owner.Id,
                StoreName = "GearZone Official Store",
                Slug = "gearzone-official",
                BusinessType = BusinessType.Individual,
                Phone = "0123456789",
                Email = "official@gearzone.com",
                AddressLine = "123 Tech Street",
                Province = "Ho Chi Minh City",
                Status = StoreStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };
            context.Stores.Add(store);

            // 3. Create Categories
            var gpuCategory = new Category { Name = "Graphics Cards", Slug = "graphics-cards", IsActive = true };
            var cpuCategory = new Category { Name = "Processors", Slug = "processors", IsActive = true };
            context.Categories.AddRange(gpuCategory, cpuCategory);
            await context.SaveChangesAsync();

            // 4. Create Brands
            var nvidia = new Brand { Name = "NVIDIA", Slug = "nvidia", IsApproved = true };
            var asus = new Brand { Name = "ASUS", Slug = "asus", IsApproved = true };
            var msi = new Brand { Name = "MSI", Slug = "msi", IsApproved = true };
            var gigabyte = new Brand { Name = "Gigabyte", Slug = "gigabyte", IsApproved = true };
            context.Brands.AddRange(nvidia, asus, msi, gigabyte);

            // 5. Create Category Attributes for GPU
            var vramAttr = new CategoryAttribute { CategoryId = gpuCategory.Id, Name = "VRAM", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 1 };
            var seriesAttr = new CategoryAttribute { CategoryId = gpuCategory.Id, Name = "Series", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 2 };
            context.CategoryAttributes.AddRange(vramAttr, seriesAttr);
            await context.SaveChangesAsync();

            // 6. Create Products
            var products = new List<Product>
            {
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = asus.Id,
                    Name = "ASUS ROG Strix GeForce RTX 4090", Slug = "asus-rog-strix-rtx-4090",
                    BasePrice = 55000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow,
                    Description = "The ultimate gaming graphics card."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = msi.Id,
                    Name = "MSI GeForce RTX 4080 SUPRIM X", Slug = "msi-rtx-4080-suprim-x",
                    BasePrice = 38000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow,
                    Description = "High performance graphics for enthusiasts."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = gigabyte.Id,
                    Name = "Gigabyte RTX 3060 Eagle OC", Slug = "gigabyte-rtx-3060-eagle-oc",
                    BasePrice = 8500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow,
                    Description = "Excellent mid-range performance."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = asus.Id,
                    Name = "ASUS Dual GeForce RTX 4060 Ti", Slug = "asus-dual-rtx-4060-ti",
                    BasePrice = 12500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow,
                    Description = "Compact and powerful."
                }
            };

            foreach(var p in products)
            {
                // Add a variant for each product
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    Sku = p.Slug.ToUpper() + "-V1",
                    Price = p.BasePrice,
                    StockQuantity = 10
                };
                
                // Add attribute values
                if (p.Name.Contains("4090")) {
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "24GB" });
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = seriesAttr.Id, Value = "RTX 40 Series" });
                } else if (p.Name.Contains("4080")) {
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "16GB" });
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = seriesAttr.Id, Value = "RTX 40 Series" });
                } else if (p.Name.Contains("3060")) {
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "12GB" });
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = seriesAttr.Id, Value = "RTX 30 Series" });
                } else if (p.Name.Contains("4060")) {
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "8GB" });
                    variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = seriesAttr.Id, Value = "RTX 40 Series" });
                }

                p.Variants.Add(variant);
                context.Products.Add(p);
            }

            await context.SaveChangesAsync();
        }
    }
}
