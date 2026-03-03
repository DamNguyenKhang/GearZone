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

            // 3. Fetch Categories (Seeded by CategorySeeder)
            var gpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gpus");
            var cpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "cpus");
            var monitorCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gaming-monitors");

            if (gpuCategory == null || cpuCategory == null || monitorCategory == null) 
            {
                return; // Guard clause in case migrations haven't run properly
            }

            // 4. Create Brands
            var nvidia = new Brand { Name = "NVIDIA", Slug = "nvidia", IsApproved = true };
            var asus = new Brand { Name = "ASUS", Slug = "asus", IsApproved = true };
            var msi = new Brand { Name = "MSI", Slug = "msi", IsApproved = true };
            var gigabyte = new Brand { Name = "Gigabyte", Slug = "gigabyte", IsApproved = true };
            var intel = new Brand { Name = "Intel", Slug = "intel", IsApproved = true };
            var amd = new Brand { Name = "AMD", Slug = "amd", IsApproved = true };
            var samsung = new Brand { Name = "Samsung", Slug = "samsung", IsApproved = true };
            var lg = new Brand { Name = "LG", Slug = "lg", IsApproved = true };
            
            context.Brands.AddRange(nvidia, asus, msi, gigabyte, intel, amd, samsung, lg);
            await context.SaveChangesAsync();

            // 5. Create Category Attributes
            // GPU Attributes
            var vramAttr = new CategoryAttribute { CategoryId = gpuCategory.Id, Name = "VRAM", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 1 };
            var gpuSeriesAttr = new CategoryAttribute { CategoryId = gpuCategory.Id, Name = "Series", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 2 };
            // CPU Attributes
            var socketAttr = new CategoryAttribute { CategoryId = cpuCategory.Id, Name = "Socket", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 1 };
            var coresAttr = new CategoryAttribute { CategoryId = cpuCategory.Id, Name = "Cores", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 2 };
            // Monitor Attributes
            var resolutionAttr = new CategoryAttribute { CategoryId = monitorCategory.Id, Name = "Resolution", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 1 };
            var refreshRateAttr = new CategoryAttribute { CategoryId = monitorCategory.Id, Name = "Refresh Rate", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 2 };
            var panelAttr = new CategoryAttribute { CategoryId = monitorCategory.Id, Name = "Panel Type", FilterType = "Checkbox", IsFilterable = true, DisplayOrder = 3 };

            context.CategoryAttributes.AddRange(vramAttr, gpuSeriesAttr, socketAttr, coresAttr, resolutionAttr, refreshRateAttr, panelAttr);
            await context.SaveChangesAsync();

            // 6. Create Products
            var products = new List<Product>
            {
                // GPUs
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = asus.Id,
                    Name = "ASUS ROG Strix GeForce RTX 4090", Slug = "asus-rog-strix-rtx-4090",
                    BasePrice = 55000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 120,
                    Description = "The ultimate gaming graphics card."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = msi.Id,
                    Name = "MSI GeForce RTX 4080 SUPRIM X", Slug = "msi-rtx-4080-suprim-x",
                    BasePrice = 38000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 85,
                    Description = "High performance graphics for enthusiasts."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = gigabyte.Id,
                    Name = "Gigabyte RTX 4070 Ti Gaming OC", Slug = "gigabyte-rtx-4070-ti-gaming-oc",
                    BasePrice = 24500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 200,
                    Description = "Sweet spot for 1440p high-refresh gaming."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = asus.Id,
                    Name = "ASUS Dual GeForce RTX 4060 Ti", Slug = "asus-dual-rtx-4060-ti",
                    BasePrice = 12500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 350,
                    Description = "Compact and powerful."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = gpuCategory.Id, BrandId = gigabyte.Id,
                    Name = "Gigabyte Radeon RX 7900 XTX", Slug = "gigabyte-rx-7900-xtx",
                    BasePrice = 31000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 45,
                    Description = "Flagship AMD RDNA 3 architecture."
                },

                // CPUs
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = cpuCategory.Id, BrandId = intel.Id,
                    Name = "Intel Core i9-14900K", Slug = "intel-core-i9-14900k",
                    BasePrice = 15500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 90,
                    Description = "24 cores, 32 threads, blazing fast."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = cpuCategory.Id, BrandId = intel.Id,
                    Name = "Intel Core i5-13600K", Slug = "intel-core-i5-13600k",
                    BasePrice = 7800000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 410,
                    Description = "The best value gaming CPU."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = cpuCategory.Id, BrandId = amd.Id,
                    Name = "AMD Ryzen 7 7800X3D", Slug = "amd-ryzen-7-7800x3d",
                    BasePrice = 10500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 310,
                    Description = "The current king of gaming CPUs with 3D V-Cache."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = cpuCategory.Id, BrandId = amd.Id,
                    Name = "AMD Ryzen 5 7600X", Slug = "amd-ryzen-5-7600x",
                    BasePrice = 6200000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 180,
                    Description = "Entry level AM5 performance."
                },

                // Monitors
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = monitorCategory.Id, BrandId = asus.Id,
                    Name = "ASUS ROG Swift OLED PG27AQDM", Slug = "asus-rog-swift-oled-pg27aqdm",
                    BasePrice = 28000000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 30,
                    Description = "27-inch 1440p OLED 240Hz monitor."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = monitorCategory.Id, BrandId = samsung.Id,
                    Name = "Samsung Odyssey G7 27\"", Slug = "samsung-odyssey-g7-27",
                    BasePrice = 14500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 150,
                    Description = "240Hz 1440p curved gaming monitor."
                },
                new Product { 
                    Id = Guid.NewGuid(), StoreId = store.Id, CategoryId = monitorCategory.Id, BrandId = lg.Id,
                    Name = "LG 27GP850-B UltraGear", Slug = "lg-27gp850-b",
                    BasePrice = 9500000, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, SoldCount = 280,
                    Description = "165Hz Nano IPS classic."
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
                    StockQuantity = new Random().Next(5, 50),
                    IsActive = true
                };
                
                // Add attribute values based on category & name
                if (p.CategoryId == gpuCategory.Id) 
                {
                    if (p.Name.Contains("4090")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "24GB" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, Value = "RTX 40 Series" });
                    } else if (p.Name.Contains("4080")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "16GB" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, Value = "RTX 40 Series" });
                    } else if (p.Name.Contains("4070")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "12GB" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, Value = "RTX 40 Series" });
                    } else if (p.Name.Contains("4060")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "8GB" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, Value = "RTX 40 Series" });
                    } else if (p.Name.Contains("7900 XTX")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, Value = "24GB" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, Value = "Radeon RX 7000" });
                    }
                }
                else if (p.CategoryId == cpuCategory.Id)
                {
                    if (p.Name.Contains("14900K") || p.Name.Contains("13600K")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = socketAttr.Id, Value = "LGA 1700" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = coresAttr.Id, Value = p.Name.Contains("14900K") ? "24 Cores" : "14 Cores" });
                    } else if (p.Name.Contains("7800X3D") || p.Name.Contains("7600X")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = socketAttr.Id, Value = "AM5" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = coresAttr.Id, Value = p.Name.Contains("7800X3D") ? "8 Cores" : "6 Cores" });
                    }
                }
                else if (p.CategoryId == monitorCategory.Id)
                {
                    if (p.Name.Contains("OLED")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, Value = "1440p (2K)" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, Value = "240Hz" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, Value = "OLED" });
                    } else if (p.Name.Contains("G7")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, Value = "1440p (2K)" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, Value = "240Hz" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, Value = "VA" });
                    } else if (p.Name.Contains("LG")) {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, Value = "1440p (2K)" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, Value = "165Hz" });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, Value = "IPS" });
                    }
                }

                p.Variants.Add(variant);
                context.Products.Add(p);
            }

            await context.SaveChangesAsync();
        }
    }
}
