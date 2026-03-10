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
            // 1. Get a user for store owner (Super Admin from identity seeder)
            var owner = await context.Users.FirstOrDefaultAsync();
            if (owner == null) return;

            // 2. Get or Create Dummy Store
            var store = await context.Stores.FirstOrDefaultAsync(s => s.OwnerUserId == owner.Id);
            if (store == null)
            {
                store = new Store
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
                await context.SaveChangesAsync();
            }

            // 3. Fetch Categories (Seeded by CategorySeeder)
            var gpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gpus");
            var cpuCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "cpus");
            var monitorCategory = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gaming-monitors");

            if (gpuCategory == null || cpuCategory == null || monitorCategory == null)
            {
                return; // Guard clause in case migrations haven't run properly
            }

            // 4. Create Brands (Idempotent)
            async Task<Brand> GetOrCreateBrandAsync(string name, string slug)
            {
                var existing = await context.Brands.FirstOrDefaultAsync(b => b.Slug == slug);
                if (existing != null) return existing;
                var brand = new Brand { Name = name, Slug = slug, IsApproved = true };
                context.Brands.Add(brand);
                await context.SaveChangesAsync();
                return brand;
            }

            var nvidia = await GetOrCreateBrandAsync("NVIDIA", "nvidia");
            var asus = await GetOrCreateBrandAsync("ASUS", "asus");
            var msi = await GetOrCreateBrandAsync("MSI", "msi");
            var gigabyte = await GetOrCreateBrandAsync("Gigabyte", "gigabyte");
            var intel = await GetOrCreateBrandAsync("Intel", "intel");
            var amd = await GetOrCreateBrandAsync("AMD", "amd");
            var samsung = await GetOrCreateBrandAsync("Samsung", "samsung");
            var lg = await GetOrCreateBrandAsync("LG", "lg");

            // 5. Create Category Attributes (Idempotent)
            async Task<CategoryAttribute> GetOrCreateAttrAsync(int categoryId, string name, string filterType, bool isFilterable, int displayOrder)
            {
                var existing = await context.CategoryAttributes.FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.Name == name);
                if (existing != null) return existing;
                var attr = new CategoryAttribute { CategoryId = categoryId, Name = name, FilterType = filterType, IsFilterable = isFilterable, DisplayOrder = displayOrder };
                context.CategoryAttributes.Add(attr);
                await context.SaveChangesAsync();
                return attr;
            }

            // GPU Attributes
            var vramAttr = await GetOrCreateAttrAsync(gpuCategory.Id, "VRAM", "Checkbox", true, 1);
            var gpuSeriesAttr = await GetOrCreateAttrAsync(gpuCategory.Id, "Series", "Checkbox", true, 2);
            // CPU Attributes
            var socketAttr = await GetOrCreateAttrAsync(cpuCategory.Id, "Socket", "Checkbox", true, 1);
            var coresAttr = await GetOrCreateAttrAsync(cpuCategory.Id, "Cores", "Checkbox", true, 2);
            // Monitor Attributes
            var resolutionAttr = await GetOrCreateAttrAsync(monitorCategory.Id, "Resolution", "Checkbox", true, 1);
            var refreshRateAttr = await GetOrCreateAttrAsync(monitorCategory.Id, "Refresh Rate", "Checkbox", true, 2);
            var panelAttr = await GetOrCreateAttrAsync(monitorCategory.Id, "Panel Type", "Checkbox", true, 3);

            // 5b. Create Options for Attributes (Idempotent)
            async Task<CategoryAttributeOption> GetOrCreateOptionAsync(int attrId, string value)
            {
                var existing = await context.CategoryAttributeOptions.FirstOrDefaultAsync(o => o.CategoryAttributeId == attrId && o.Value == value);
                if (existing != null) return existing;
                var option = new CategoryAttributeOption { CategoryAttributeId = attrId, Value = value };
                context.CategoryAttributeOptions.Add(option);
                await context.SaveChangesAsync();
                return option;
            }

            // GPU VRAM
            var vram8 = await GetOrCreateOptionAsync(vramAttr.Id, "8GB");
            var vram12 = await GetOrCreateOptionAsync(vramAttr.Id, "12GB");
            var vram16 = await GetOrCreateOptionAsync(vramAttr.Id, "16GB");
            var vram24 = await GetOrCreateOptionAsync(vramAttr.Id, "24GB");
            // GPU Series
            var rtx40 = await GetOrCreateOptionAsync(gpuSeriesAttr.Id, "RTX 40 Series");
            var rx7000 = await GetOrCreateOptionAsync(gpuSeriesAttr.Id, "Radeon RX 7000");
            // CPU Sockets
            var lga1700 = await GetOrCreateOptionAsync(socketAttr.Id, "LGA 1700");
            var am5 = await GetOrCreateOptionAsync(socketAttr.Id, "AM5");
            // CPU Cores
            var cores6 = await GetOrCreateOptionAsync(coresAttr.Id, "6 Cores");
            var cores8 = await GetOrCreateOptionAsync(coresAttr.Id, "8 Cores");
            var cores14 = await GetOrCreateOptionAsync(coresAttr.Id, "14 Cores");
            var cores24 = await GetOrCreateOptionAsync(coresAttr.Id, "24 Cores");
            // Monitor Res
            var res2k = await GetOrCreateOptionAsync(resolutionAttr.Id, "1440p (2K)");
            // Monitor Refresh
            var hz165 = await GetOrCreateOptionAsync(refreshRateAttr.Id, "165Hz");
            var hz240 = await GetOrCreateOptionAsync(refreshRateAttr.Id, "240Hz");
            // Monitor Panel
            var ips = await GetOrCreateOptionAsync(panelAttr.Id, "IPS");
            var va = await GetOrCreateOptionAsync(panelAttr.Id, "VA");
            var oled = await GetOrCreateOptionAsync(panelAttr.Id, "OLED");

            var options = new List<CategoryAttributeOption> { vram8, vram12, vram16, vram24, rtx40, rx7000, lga1700, am5, cores6, cores8, cores14, cores24, res2k, hz165, hz240, ips, va, oled };

            // Helper to find option id
            int GetOptionId(int attrId, string val) => options.First(o => o.CategoryAttributeId == attrId && o.Value == val).Id;

            // 6. Create Products if they don't exist
            var productsToAdd = new List<Product>();
            var sampleProducts = new List<Product>
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

            foreach (var p in sampleProducts)
            {
                if (await context.Products.AnyAsync(existing => existing.Slug == p.Slug)) continue;

                productsToAdd.Add(p);
            }

            foreach (var p in productsToAdd)
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
                    if (p.Name.Contains("4090"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, CategoryAttributeOptionId = GetOptionId(vramAttr.Id, "24GB") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, CategoryAttributeOptionId = GetOptionId(gpuSeriesAttr.Id, "RTX 40 Series") });
                    }
                    else if (p.Name.Contains("4080"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, CategoryAttributeOptionId = GetOptionId(vramAttr.Id, "16GB") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, CategoryAttributeOptionId = GetOptionId(gpuSeriesAttr.Id, "RTX 40 Series") });
                    }
                    else if (p.Name.Contains("4070"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, CategoryAttributeOptionId = GetOptionId(vramAttr.Id, "12GB") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, CategoryAttributeOptionId = GetOptionId(gpuSeriesAttr.Id, "RTX 40 Series") });
                    }
                    else if (p.Name.Contains("4060"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, CategoryAttributeOptionId = GetOptionId(vramAttr.Id, "8GB") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, CategoryAttributeOptionId = GetOptionId(gpuSeriesAttr.Id, "RTX 40 Series") });
                    }
                    else if (p.Name.Contains("7900 XTX"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = vramAttr.Id, CategoryAttributeOptionId = GetOptionId(vramAttr.Id, "24GB") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = gpuSeriesAttr.Id, CategoryAttributeOptionId = GetOptionId(gpuSeriesAttr.Id, "Radeon RX 7000") });
                    }
                }
                else if (p.CategoryId == cpuCategory.Id)
                {
                    if (p.Name.Contains("14900K") || p.Name.Contains("13600K"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = socketAttr.Id, CategoryAttributeOptionId = GetOptionId(socketAttr.Id, "LGA 1700") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = coresAttr.Id, CategoryAttributeOptionId = GetOptionId(coresAttr.Id, p.Name.Contains("14900K") ? "24 Cores" : "14 Cores") });
                    }
                    else if (p.Name.Contains("7800X3D") || p.Name.Contains("7600X"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = socketAttr.Id, CategoryAttributeOptionId = GetOptionId(socketAttr.Id, "AM5") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = coresAttr.Id, CategoryAttributeOptionId = GetOptionId(coresAttr.Id, p.Name.Contains("7800X3D") ? "8 Cores" : "6 Cores") });
                    }
                }
                else if (p.CategoryId == monitorCategory.Id)
                {
                    if (p.Name.Contains("OLED"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, CategoryAttributeOptionId = GetOptionId(resolutionAttr.Id, "1440p (2K)") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, CategoryAttributeOptionId = GetOptionId(refreshRateAttr.Id, "240Hz") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, CategoryAttributeOptionId = GetOptionId(panelAttr.Id, "OLED") });
                    }
                    else if (p.Name.Contains("G7"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, CategoryAttributeOptionId = GetOptionId(resolutionAttr.Id, "1440p (2K)") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, CategoryAttributeOptionId = GetOptionId(refreshRateAttr.Id, "240Hz") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, CategoryAttributeOptionId = GetOptionId(panelAttr.Id, "VA") });
                    }
                    else if (p.Name.Contains("LG"))
                    {
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = resolutionAttr.Id, CategoryAttributeOptionId = GetOptionId(resolutionAttr.Id, "1440p (2K)") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = refreshRateAttr.Id, CategoryAttributeOptionId = GetOptionId(refreshRateAttr.Id, "165Hz") });
                        variant.AttributeValues.Add(new VariantAttributeValue { CategoryAttributeId = panelAttr.Id, CategoryAttributeOptionId = GetOptionId(panelAttr.Id, "IPS") });
                    }
                }

                p.Variants.Add(variant);
                context.Products.Add(p);
            }

            await context.SaveChangesAsync();
        }
    }
}