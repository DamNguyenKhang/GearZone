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
            
            // Log for verification
            Console.WriteLine("Seed: Database is empty, initializing catalog...");

            // ── Store ────────────────────────────────────────────────────────
            var store = new Store
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

            // ── Attribute helpers ────────────────────────────────────────────
            CategoryAttribute Attr(Category cat, string name, int order, AttributeScope scope = AttributeScope.Variant, bool isComparable = false, string? valueType = null, string? unit = null) =>
                new CategoryAttribute { 
                    CategoryId = cat.Id, 
                    Name = name, 
                    FilterType = "Checkbox", 
                    IsFilterable = true, 
                    DisplayOrder = order,
                    Scope = scope,
                    IsComparable = isComparable,
                    ValueType = valueType,
                    Unit = unit
                };
            CategoryAttributeOption Opt(CategoryAttribute a, string val) =>
                new CategoryAttributeOption { CategoryAttributeId = a.Id, Value = val };

            // ── GPU Attributes ───────────────────────────────────────────────
            var gpuVram   = Attr(catGpu, "VRAM", 1, AttributeScope.Both, true, "String", "GB");
            var gpuSeries = Attr(catGpu, "Series", 2, AttributeScope.Both, true);
            var gpuCuda   = Attr(catGpu, "CUDA Cores", 3, AttributeScope.Product, true, "Number");
            var gpuClock  = Attr(catGpu, "Boost Clock", 4, AttributeScope.Product, true, "Number", "MHz");
            var gpuTdp    = Attr(catGpu, "TDP", 5, AttributeScope.Product, true, "Number", "W");
            var gpuNode   = Attr(catGpu, "Process Node", 6, AttributeScope.Product, true);

            // ── CPU Attributes ───────────────────────────────────────────────
            var cpuSocket = Attr(catCpu, "Socket", 1, AttributeScope.Both, true);
            var cpuCores  = Attr(catCpu, "Cores", 2, AttributeScope.Both, true, "Number");
            var cpuThreads = Attr(catCpu, "Threads", 3, AttributeScope.Product, true, "Number");
            var cpuBaseClock = Attr(catCpu, "Base Clock", 4, AttributeScope.Product, true, "Number", "GHz");
            var cpuBoostClock  = Attr(catCpu, "Boost Clock", 5, AttributeScope.Product, true, "Number", "GHz");
            var cpuCache  = Attr(catCpu, "L3 Cache", 6, AttributeScope.Product, true, "Number", "MB");
            var cpuTdp    = Attr(catCpu, "TDP", 7, AttributeScope.Product, true, "Number", "W");

            // ── RAM Attributes ───────────────────────────────────────────────
            var ramType = Attr(catRam, "Type", 1, AttributeScope.Both, true);
            var ramCap  = Attr(catRam, "Capacity", 2, AttributeScope.Both, true, "Number", "GB");

            // ── Motherboard Attributes ───────────────────────────────────────
            var moboChipset = Attr(catMobo, "Chipset", 1, AttributeScope.Both, true);
            var moboFF      = Attr(catMobo, "Form Factor", 2, AttributeScope.Both, true);

            // ── Storage Attributes ───────────────────────────────────────────
            var storType = Attr(catStorage, "Type", 1, AttributeScope.Both, true);
            var storCap  = Attr(catStorage, "Capacity", 2, AttributeScope.Both, true, "Number", "GB");

            // ── PSU Attributes ───────────────────────────────────────────────
            var psuWatt    = Attr(catPsu, "Wattage", 1, AttributeScope.Both, true, "Number", "W");
            var psuRating  = Attr(catPsu, "Efficiency Rating", 2, AttributeScope.Both, true);
            var psuModular = Attr(catPsu, "Modular", 3, AttributeScope.Both, true);

            // ── Case Attributes ──────────────────────────────────────────────
            var caseFF    = Attr(catCase, "Form Factor", 1, AttributeScope.Both, true);
            var casePanel = Attr(catCase, "Side Panel", 2, AttributeScope.Both, true);

            // ── Gaming Monitor Attributes ────────────────────────────────────
            var gamMonRes   = Attr(catGamMon, "Resolution", 1, AttributeScope.Both, true);
            var gamMonHz    = Attr(catGamMon, "Refresh Rate", 2, AttributeScope.Both, true, "Number", "Hz");
            var gamMonPanel = Attr(catGamMon, "Panel Type", 3, AttributeScope.Both, true);

            // ── Office Monitor Attributes ────────────────────────────────────
            var offMonRes   = catOffMon != null ? Attr(catOffMon, "Resolution", 1, AttributeScope.Both, true) : null;
            var offMonPanel = catOffMon != null ? Attr(catOffMon, "Panel Type", 2, AttributeScope.Both, true) : null;
            var offMonSize  = catOffMon != null ? Attr(catOffMon, "Screen Size", 3, AttributeScope.Both, true, "Number", "inch") : null;

            // ── Curved Monitor Attributes ────────────────────────────────────
            var crvMonRes   = catCrvMon != null ? Attr(catCrvMon, "Resolution", 1, AttributeScope.Both, true) : null;
            var crvMonHz    = catCrvMon != null ? Attr(catCrvMon, "Refresh Rate", 2, AttributeScope.Both, true, "Number", "Hz") : null;
            var crvMonPanel = catCrvMon != null ? Attr(catCrvMon, "Panel Type", 3, AttributeScope.Both, true) : null;

            // ── Mechanical Keyboard Attributes ───────────────────────────────
            var mechSwitch = Attr(catMechKb, "Switch Type", 1, AttributeScope.Both, true);
            var mechLayout = Attr(catMechKb, "Layout", 2, AttributeScope.Both, true);

            // ── Membrane Keyboard Attributes ─────────────────────────────────
            var membLayout = catMembKb != null ? Attr(catMembKb, "Layout", 1, AttributeScope.Both, true) : null;
            var membConn   = catMembKb != null ? Attr(catMembKb, "Connectivity", 2, AttributeScope.Both, true) : null;

            // ── Keycap Attributes ────────────────────────────────────────────
            var keycapMat  = catKeycaps != null ? Attr(catKeycaps, "Material", 1, AttributeScope.Both, true) : null;
            var keycapProf = catKeycaps != null ? Attr(catKeycaps, "Profile", 2, AttributeScope.Both, true) : null;

            // ── Keyboard Switch Attributes ───────────────────────────────────
            var kbswType   = catKbSwitch != null ? Attr(catKbSwitch, "Switch Type", 1, AttributeScope.Both, true) : null;
            var kbswActu   = catKbSwitch != null ? Attr(catKbSwitch, "Actuation Force", 2, AttributeScope.Both, true) : null;

            // ── Gaming Mouse Attributes ──────────────────────────────────────
            var gamMouseDpi  = Attr(catGamMice, "Max DPI", 1, AttributeScope.Both, true, "Number", "DPI");
            var gamMouseConn = Attr(catGamMice, "Connectivity", 2, AttributeScope.Both, true);
            var gamMouseColor = Attr(catGamMice, "Color", 3, AttributeScope.Both, true);

            // ── Office Mouse Attributes ──────────────────────────────────────
            var offMouseConn   = catOffMice != null ? Attr(catOffMice, "Connectivity", 1, AttributeScope.Both, true) : null;
            var offMouseSensor = catOffMice != null ? Attr(catOffMice, "Sensor Type", 2, AttributeScope.Both, true) : null;

            // ── Mouse Pad Attributes ─────────────────────────────────────────
            var padSize = catMousePad != null ? Attr(catMousePad, "Size", 1, AttributeScope.Both, true) : null;
            var padMat  = catMousePad != null ? Attr(catMousePad, "Material", 2, AttributeScope.Both, true) : null;

            // ── Gaming Headset Attributes ────────────────────────────────────
            var gamHsConn  = Attr(catGamHS, "Connectivity", 1, AttributeScope.Both, true);
            var gamHsSurr  = Attr(catGamHS, "Surround Sound", 2, AttributeScope.Both, true);

            // ── Wireless Headphone Attributes ────────────────────────────────
            var wirHpAnc    = catWirelessHP != null ? Attr(catWirelessHP, "ANC", 1, AttributeScope.Both, true) : null;
            var wirHpDriver = catWirelessHP != null ? Attr(catWirelessHP, "Driver Size", 2, AttributeScope.Both, true, "Number", "mm") : null;

            // ── Microphone Attributes ────────────────────────────────────────
            var micType    = catMic != null ? Attr(catMic, "Connection Type", 1, AttributeScope.Both, true) : null;
            var micPattern = catMic != null ? Attr(catMic, "Polar Pattern", 2, AttributeScope.Both, true) : null;

            // ── Save all attributes ──────────────────────────────────────────
            var allAttrs = new List<CategoryAttribute>
            {
                gpuVram, gpuSeries, gpuCuda, gpuClock, gpuTdp, gpuNode,
                cpuSocket, cpuCores, cpuThreads, cpuBaseClock, cpuBoostClock, cpuCache, cpuTdp,
                ramType, ramCap,
                moboChipset, moboFF, storType, storCap, psuWatt, psuRating, psuModular,
                caseFF, casePanel, gamMonRes, gamMonHz, gamMonPanel,
                mechSwitch, mechLayout, gamMouseDpi, gamMouseConn, gamMouseColor, gamHsConn, gamHsSurr
            };
            void AddIfNotNull(params CategoryAttribute?[] attrs) { foreach (var a in attrs) if (a != null) allAttrs.Add(a); }
            AddIfNotNull(offMonRes, offMonPanel, offMonSize, crvMonRes, crvMonHz, crvMonPanel,
                         membLayout, membConn, keycapMat, keycapProf, kbswType, kbswActu,
                         offMouseConn, offMouseSensor, padSize, padMat,
                         wirHpAnc, wirHpDriver, micType, micPattern);
            context.CategoryAttributes.AddRange(allAttrs);
            await context.SaveChangesAsync();

            // ── Attribute Options ────────────────────────────────────────────
            var opts = new List<CategoryAttributeOption>();
            CategoryAttributeOption O(CategoryAttribute a, string v) { var o = Opt(a, v); opts.Add(o); return o; }

            // GPU
            var vram8  = O(gpuVram,"8GB");  var vram12 = O(gpuVram,"12GB");
            var vram16 = O(gpuVram,"16GB"); var vram24 = O(gpuVram,"24GB");
            var rtx40  = O(gpuSeries,"RTX 40 Series"); var rx7000 = O(gpuSeries,"Radeon RX 7000");

            // CPU
            var lga1700 = O(cpuSocket,"LGA 1700"); var am5  = O(cpuSocket,"AM5");
            var cpu6    = O(cpuCores,"6 Cores");   var cpu8  = O(cpuCores,"8 Cores");
            var cpu14   = O(cpuCores,"14 Cores");  var cpu24 = O(cpuCores,"24 Cores");

            // RAM
            var ramDdr4 = O(ramType,"DDR4"); var ramDdr5 = O(ramType,"DDR5");
            var ram16   = O(ramCap,"16GB");  var ram32   = O(ramCap,"32GB"); var ram64 = O(ramCap,"64GB");

            // Motherboard
            var chipZ790 = O(moboChipset,"Z790"); var chipB760 = O(moboChipset,"B760");
            var chipX670 = O(moboChipset,"X670E"); var chipB650 = O(moboChipset,"B650");
            var ffAtx = O(moboFF,"ATX"); var ffMAtx = O(moboFF,"mATX"); var ffItx = O(moboFF,"ITX");

            // Storage
            var storNvme = O(storType,"NVMe SSD"); var storSata = O(storType,"SATA SSD"); var storHdd = O(storType,"HDD");
            var stor512  = O(storCap,"512GB");     var stor1tb  = O(storCap,"1TB");       var stor2tb = O(storCap,"2TB");

            // PSU
            var psu650  = O(psuWatt,"650W"); var psu750 = O(psuWatt,"750W");
            var psu850  = O(psuWatt,"850W"); var psu1000 = O(psuWatt,"1000W+");
            var ratBron = O(psuRating,"80+ Bronze"); var ratGold = O(psuRating,"80+ Gold");
            var ratPlat = O(psuRating,"80+ Platinum"); var ratTit = O(psuRating,"80+ Titanium");
            var modNo   = O(psuModular,"Non-Modular"); var modSemi = O(psuModular,"Semi-Modular"); var modFull = O(psuModular,"Fully Modular");

            // Cases
            var caseAtx  = O(caseFF,"ATX Mid Tower"); var caseMAtx = O(caseFF,"mATX Mid Tower");
            var caseItx  = O(caseFF,"ITX Mini Tower"); var caseFull = O(caseFF,"Full Tower");
            var panelGlass = O(casePanel,"Tempered Glass"); var panelSteel = O(casePanel,"Steel");

            // Gaming Monitors
            var gm1080  = O(gamMonRes,"1080p (FHD)"); var gm2k = O(gamMonRes,"1440p (2K)"); var gm4k = O(gamMonRes,"4K (UHD)");
            var ghz144  = O(gamMonHz,"144Hz"); var ghz165 = O(gamMonHz,"165Hz");
            var ghz240  = O(gamMonHz,"240Hz"); var ghz360 = O(gamMonHz,"360Hz");
            var gips = O(gamMonPanel,"IPS"); var gva = O(gamMonPanel,"VA"); var goled = O(gamMonPanel,"OLED"); var gtn = O(gamMonPanel,"TN");

            // Office Monitors
            CategoryAttributeOption? om1080 = null, om2k = null, om4k = null, omIps = null, omVa = null, om24 = null, om27 = null, om32 = null;
            if (offMonRes != null)
            {
                om1080 = O(offMonRes,"1080p (FHD)"); om2k = O(offMonRes,"1440p (2K)"); om4k = O(offMonRes,"4K (UHD)");
                omIps  = O(offMonPanel!,"IPS"); omVa = O(offMonPanel!,"VA");
                om24   = O(offMonSize!,"24\""); om27 = O(offMonSize!,"27\""); om32 = O(offMonSize!,"32\"");
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

            // ── GPUs ─────────────────────────────────────────────────────────
            Add(P(catGpu,asus,"ASUS ROG Strix GeForce RTX 4090","asus-rog-strix-rtx-4090",55000000,120), "{\"Process Node\":\"4 nm\",\"CUDA Cores\":\"16384\",\"Boost Clock\":\"2550 MHz\",\"Max Resolution\":\"7680x4320\",\"TDP\":\"450W\"}", Av(gpuVram,vram24),Av(gpuSeries,rtx40));
            Add(P(catGpu,msi,"MSI GeForce RTX 4080 SUPRIM X","msi-rtx-4080-suprim-x",38000000,85), null, Av(gpuVram,vram16),Av(gpuSeries,rtx40));
            Add(P(catGpu,gigabyte,"Gigabyte RTX 4070 Ti Gaming OC","gigabyte-rtx-4070-ti-gaming-oc",24500000,200), null,  Av(gpuVram,vram12),Av(gpuSeries,rtx40));
            Add(P(catGpu,asus,"ASUS Dual GeForce RTX 4060 Ti","asus-dual-rtx-4060-ti",12500000,350), null, Av(gpuVram,vram8), Av(gpuSeries,rtx40));
            Add(P(catGpu,gigabyte,"Gigabyte Radeon RX 7900 XTX","gigabyte-rx-7900-xtx",31000000,45), null, Av(gpuVram,vram24),Av(gpuSeries,rx7000));
            Add(P(catGpu,msi,"MSI Radeon RX 7800 XT Gaming X Trio","msi-rx-7800-xt",17500000,130), null, Av(gpuVram,vram16),Av(gpuSeries,rx7000));

            // ── CPUs ─────────────────────────────────────────────────────────
            Add(P(catCpu,intel,"Intel Core i9-14900K","intel-core-i9-14900k",15500000,90), "{\"Threads\":\"32\",\"Base Clock\":\"3.2 GHz\",\"Max Boost Clock\":\"6.0 GHz\",\"L3 Cache\":\"36MB\",\"TDP\":\"125W\"}", Av(cpuSocket,lga1700),Av(cpuCores,cpu24));
            Add(P(catCpu,intel,"Intel Core i7-14700K","intel-core-i7-14700k",11200000,160), null,  Av(cpuSocket,lga1700),Av(cpuCores,cpu14));
            Add(P(catCpu,intel,"Intel Core i5-13600K","intel-core-i5-13600k",7800000,410), null,   Av(cpuSocket,lga1700),Av(cpuCores,cpu14));
            Add(P(catCpu,amd,"AMD Ryzen 9 7950X3D","amd-ryzen-9-7950x3d",19000000,40), null,       Av(cpuSocket,am5),    Av(cpuCores,cpu24));
            Add(P(catCpu,amd,"AMD Ryzen 7 7800X3D","amd-ryzen-7-7800x3d",10500000,310), null,      Av(cpuSocket,am5),    Av(cpuCores,cpu8));
            Add(P(catCpu,amd,"AMD Ryzen 5 7600X","amd-ryzen-5-7600x",6200000,180), null,           Av(cpuSocket,am5),    Av(cpuCores,cpu6));

            // ── RAM ──────────────────────────────────────────────────────────
            AddMulti(P(catRam,corsair,"Corsair Vengeance DDR5-5600","corsair-vengeance-ddr5-5600",1800000,320), "{\"Memory Type\":\"DDR5\",\"Latency\":\"CL36\",\"Voltage\":\"1.25V\",\"Heat Spreader\":\"Aluminum\",\"RGB\":\"No\"}",
                (0m, new[] { Av(ramType,ramDdr5), Av(ramCap,ram16) }),
                (1700000m, new[] { Av(ramType,ramDdr5), Av(ramCap,ram32) })
            );
            Add(P(catRam,kingston,"Kingston Fury Beast DDR5-4800 32GB","kingston-fury-beast-ddr5-32gb",3100000,250), null, Av(ramType,ramDdr5),Av(ramCap,ram32));
            Add(P(catRam,crucial,"Crucial DDR4-3200 32GB Kit","crucial-ddr4-3200-32gb",1500000,600), null,              Av(ramType,ramDdr4),Av(ramCap,ram32));
            Add(P(catRam,corsair,"Corsair Dominator Platinum DDR5 64GB","corsair-dominator-ddr5-64gb",7800000,50), null, Av(ramType,ramDdr5),Av(ramCap,ram64));

            // ── Motherboards ─────────────────────────────────────────────────
            Add(P(catMobo,asus,"ASUS ROG Maximus Z790 Hero","asus-rog-maximus-z790-hero",15800000,28), null,           Av(moboChipset,chipZ790),Av(moboFF,ffAtx));
            Add(P(catMobo,msi,"MSI MAG Z790 Tomahawk WiFi","msi-mag-z790-tomahawk",5200000,95), null,                  Av(moboChipset,chipZ790),Av(moboFF,ffAtx));
            Add(P(catMobo,gigabyte,"Gigabyte B760M DS3H mATX","gigabyte-b760m-ds3h",2400000,220), null,               Av(moboChipset,chipB760),Av(moboFF,ffMAtx));
            Add(P(catMobo,asus,"ASUS ROG Crosshair X670E Extreme","asus-rog-crosshair-x670e",19500000,15), null,       Av(moboChipset,chipX670),Av(moboFF,ffAtx));
            Add(P(catMobo,msi,"MSI PRO B650M-A WiFi","msi-pro-b650m-a-wifi",3200000,140), null,                       Av(moboChipset,chipB650),Av(moboFF,ffMAtx));

            // ── Storage ──────────────────────────────────────────────────────
            Add(P(catStorage,samsung,"Samsung 980 Pro NVMe 1TB","samsung-980-pro-nvme-1tb",3200000,580), null,     Av(storType,storNvme),Av(storCap,stor1tb));
            Add(P(catStorage,samsung,"Samsung 870 EVO SATA SSD 1TB","samsung-870-evo-sata-1tb",1900000,750), null, Av(storType,storSata),Av(storCap,stor1tb));
            Add(P(catStorage,wd,"WD Black SN850X NVMe 2TB","wd-black-sn850x-2tb",5500000,210), null,              Av(storType,storNvme),Av(storCap,stor2tb));
            Add(P(catStorage,seagate,"Seagate Barracuda HDD 2TB","seagate-barracuda-2tb",900000,900), null,        Av(storType,storHdd), Av(storCap,stor2tb));
            Add(P(catStorage,crucial,"Crucial P3 NVMe 1TB","crucial-p3-nvme-1tb",1800000,420), null,              Av(storType,storNvme),Av(storCap,stor1tb));
            Add(P(catStorage,samsung,"Samsung 990 Pro NVMe 512GB","samsung-990-pro-512gb",2100000,310), null,      Av(storType,storNvme),Av(storCap,stor512));

            // ── PSU ──────────────────────────────────────────────────────────
            Add(P(catPsu,seasonic,"Seasonic Focus GX-850W 80+ Gold","seasonic-focus-gx-850",3500000,180), null,          Av(psuWatt,psu850), Av(psuRating,ratGold), Av(psuModular,modFull));
            Add(P(catPsu,corsair,"Corsair RM1000x 80+ Gold Fully Modular","corsair-rm1000x",4800000,90), null,            Av(psuWatt,psu1000),Av(psuRating,ratGold), Av(psuModular,modFull));
            Add(P(catPsu,evga,"EVGA SuperNOVA 750W G6 80+ Gold","evga-supernova-750-g6",2900000,130), null,              Av(psuWatt,psu750), Av(psuRating,ratGold), Av(psuModular,modFull));
            Add(P(catPsu,msi,"MSI MAG A650BN 650W 80+ Bronze","msi-mag-a650bn",1800000,290), null,                       Av(psuWatt,psu650), Av(psuRating,ratBron), Av(psuModular,modSemi));
            Add(P(catPsu,seasonic,"Seasonic Prime TX-1000W 80+ Titanium","seasonic-prime-tx-1000",8500000,40), null,      Av(psuWatt,psu1000),Av(psuRating,ratTit),  Av(psuModular,modFull));
            Add(P(catPsu,corsair,"Corsair SF750 80+ Platinum SFX","corsair-sf750",4200000,65), null,                      Av(psuWatt,psu750), Av(psuRating,ratPlat), Av(psuModular,modFull));

            // ── PC Cases ─────────────────────────────────────────────────────
            Add(P(catCase,nzxt,"NZXT H7 Flow RGB Mid-Tower","nzxt-h7-flow-rgb",3200000,220), null,                     Av(caseFF,caseAtx), Av(casePanel,panelGlass));
            Add(P(catCase,fractal,"Fractal Design North ATX Mid Tower","fractal-design-north",3800000,85), null,         Av(caseFF,caseAtx), Av(casePanel,panelGlass));
            Add(P(catCase,corsair,"Corsair 4000D Airflow Tempered Glass","corsair-4000d-airflow",2100000,340), null,     Av(caseFF,caseAtx), Av(casePanel,panelGlass));
            Add(P(catCase,asus,"ASUS Prime AP201 Mesh mATX","asus-prime-ap201",1600000,150), null,                      Av(caseFF,caseMAtx),Av(casePanel,panelSteel));
            Add(P(catCase,lianli,"Lian Li PC-O11 Dynamic EVO Full Tower","lianli-o11-dynamic-evo",4500000,95), null,    Av(caseFF,caseFull),Av(casePanel,panelGlass));
            Add(P(catCase,nzxt,"NZXT H1 V2 ITX Mini Tower","nzxt-h1-v2-itx",3900000,55), null,                        Av(caseFF,caseItx), Av(casePanel,panelGlass));

            // ── Gaming Monitors ──────────────────────────────────────────────
            Add(P(catGamMon,asus,"ASUS ROG Swift OLED PG27AQDM","asus-rog-swift-oled-pg27aqdm",28000000,30), null,  Av(gamMonRes,gm2k), Av(gamMonHz,ghz240),Av(gamMonPanel,goled));
            Add(P(catGamMon,samsung,"Samsung Odyssey G7 27\"","samsung-odyssey-g7-27",14500000,150), null,           Av(gamMonRes,gm2k), Av(gamMonHz,ghz240),Av(gamMonPanel,gva));
            Add(P(catGamMon,lg,"LG 27GP850-B UltraGear","lg-27gp850-b",9500000,280), null,                          Av(gamMonRes,gm2k), Av(gamMonHz,ghz165),Av(gamMonPanel,gips));
            Add(P(catGamMon,asus,"ASUS TUF Gaming VG27AQ","asus-tuf-vg27aq",8200000,420), null,                     Av(gamMonRes,gm2k), Av(gamMonHz,ghz165),Av(gamMonPanel,gips));
            Add(P(catGamMon,msi,"MSI MAG274QRF-QD 165Hz 2K","msi-mag274qrf-qd",9800000,95), null,                   Av(gamMonRes,gm2k), Av(gamMonHz,ghz165),Av(gamMonPanel,gips));
            Add(P(catGamMon,benq,"BenQ MOBIUZ EX2710Q 165Hz","benq-mobiuz-ex2710q",8900000,110), null,               Av(gamMonRes,gm2k), Av(gamMonHz,ghz165),Av(gamMonPanel,gips));
            Add(P(catGamMon,asus,"ASUS ROG XG27ACS 360Hz FHD","asus-rog-xg27acs-360hz",19500000,25), null,           Av(gamMonRes,gm1080),Av(gamMonHz,ghz360),Av(gamMonPanel,gips));

            // ── Office Monitors ──────────────────────────────────────────────
            if (catOffMon != null && om4k != null)
            {
                Add(P(catOffMon,lg,"LG 27UK850-W 4K UHD IPS","lg-27uk850-w",9200000,110), null, Av(offMonRes!,om4k),  Av(offMonPanel!,omIps!),Av(offMonSize!,om27!));
                Add(P(catOffMon,samsung,"Samsung 32\" UR550 4K UHD","samsung-ur550-32",7500000,200), null, Av(offMonRes!,om4k),  Av(offMonPanel!,omVa!), Av(offMonSize!,om32!));
                Add(P(catOffMon,asus,"ASUS ProArt PA279CV 4K 27\"","asus-proart-pa279cv",13500000,55), null, Av(offMonRes!,om4k),Av(offMonPanel!,omIps!),Av(offMonSize!,om27!));
                Add(P(catOffMon,lg,"LG 24MP60G-B FHD 75Hz","lg-24mp60g-b",2800000,380), null, Av(offMonRes!,om1080!),Av(offMonPanel!,omIps!),Av(offMonSize!,om24!));
            }

            // ── Curved Monitors ──────────────────────────────────────────────
            if (catCrvMon != null && cmRes2k != null)
            {
                Add(P(catCrvMon,samsung,"Samsung Odyssey G9 49\" Ultra-wide","samsung-odyssey-g9-49",38000000,18), null, Av(crvMonRes!,cmResUw!),Av(crvMonHz!,cmHz240!),Av(crvMonPanel!,cmVa!));
                Add(P(catCrvMon,msi,"MSI Optix MAG301CR2 30\" Curved","msi-mag301cr2",7800000,75), null, Av(crvMonRes!,cmRes2k!),Av(crvMonHz!,cmHz165!),Av(crvMonPanel!,cmVa!));
                Add(P(catCrvMon,asus,"ASUS TUF Gaming VG34VQL3A 34\"","asus-tuf-vg34vql3a",11500000,45), null, Av(crvMonRes!,cmResUw!),Av(crvMonHz!,cmHz165!),Av(crvMonPanel!,cmVa!));
                Add(P(catCrvMon,lg,"LG 38WP85C-W 38\" Ultrawide IPS","lg-38wp85c-w",29000000,20), null, Av(crvMonRes!,cmResUw!),Av(crvMonHz!,cmHz144!),Av(crvMonPanel!,cmIps!));
            }

            // ── Mechanical Keyboards ─────────────────────────────────────────
            AddMulti(P(catMechKb,keychron,"Keychron Q2 Pro 65% Wireless","keychron-q2-pro-65",3500000,190), "{\"Body Material\":\"Aluminum\",\"Backlight\":\"South-facing RGB LED\",\"Battery\":\"4000 mAh\",\"Hot-swappable\":\"Yes\",\"Polling Rate\":\"1000Hz (Wired), 90Hz (Wireless)\"}",
                (0m, new[] { Av(mechSwitch,swTactile), Av(mechLayout,lay65) }),
                (0m, new[] { Av(mechSwitch,swLinear), Av(mechLayout,lay65) }),
                (0m, new[] { Av(mechSwitch,swClicky), Av(mechLayout,lay65) })
            );
            Add(P(catMechKb,ducky,"Ducky One 3 TKL RGB","ducky-one-3-tkl",2800000,240), null,                     Av(mechSwitch,swClicky), Av(mechLayout,lay80));
            AddMulti(P(catMechKb,corsair,"Corsair K100 RGB Full Size","corsair-k100-rgb",6200000,75), null,
                (0m, new[] { Av(mechSwitch,swLinear), Av(mechLayout,lay100) }),
                (0m, new[] { Av(mechSwitch,swTactile), Av(mechLayout,lay100) })
            );
            Add(P(catMechKb,razer,"Razer BlackWidow V4 Pro TKL","razer-blackwidow-v4-pro",5200000,60), null,       Av(mechSwitch,swLinear), Av(mechLayout,lay80));
            Add(P(catMechKb,hyperx,"HyperX Alloy Origins Core TKL","hyperx-alloy-origins-tkl",1900000,310), null, Av(mechSwitch,swLinear), Av(mechLayout,lay80));

            // ── Membrane Keyboards ───────────────────────────────────────────
            if (catMembKb != null && membLay100 != null)
            {
                Add(P(catMembKb,logitech,"Logitech G213 Prodigy Gaming","logitech-g213",1200000,450), null,   Av(membLayout!,membLay100!),Av(membConn!,membWired!));
                Add(P(catMembKb,razer,"Razer Ornata V3 X","razer-ornata-v3x",900000,380), null,               Av(membLayout!,membLay100!),Av(membConn!,membWired!));
                Add(P(catMembKb,logitech,"Logitech MK295 Silent Wireless","logitech-mk295",850000,290), null, Av(membLayout!,membLay100!),Av(membConn!,membWireless!));
            }

            // ── Keycaps ──────────────────────────────────────────────────────
            if (catKeycaps != null && capPbt != null)
            {
                Add(P(catKeycaps,hyperx,"HyperX Pudding Keycaps Full Set","hyperx-pudding-keycaps",650000,520), null, Av(keycapMat!,capAbs!),Av(keycapProf!,capOem!));
                Add(P(catKeycaps,corsair,"Corsair PBT Double-Shot Pro Keycaps","corsair-pbt-keycaps",850000,280), null, Av(keycapMat!,capPbt!),Av(keycapProf!,capCherry!));
                Add(P(catKeycaps,akko,"Akko ASA Profile PBT Keycaps","akko-asa-pbt-keycaps",450000,640), null,        Av(keycapMat!,capPbt!),Av(keycapProf!,capSa!));
            }

            // ── Keyboard Switches ────────────────────────────────────────────
            if (catKbSwitch != null && kswLinear != null)
            {
                Add(P(catKbSwitch,gateron,"Gateron G Pro 3.0 Yellow (35 pcs)","gateron-g-pro-3-yellow",280000,800), null, Av(kbswType!,kswLinear!), Av(kbswActu!,kswLight!));
                Add(P(catKbSwitch,akko,"Akko CS Jelly Pink (45 pcs)","akko-cs-jelly-pink",320000,650), null, Av(kbswType!,kswLinear!), Av(kbswActu!,kswLight!));
                Add(P(catKbSwitch,gateron,"Gateron Brown Tactile (35 pcs)","gateron-brown-35pcs",280000,540), null, Av(kbswType!,kswTactile!),Av(kbswActu!,kswMed!));
                Add(P(catKbSwitch,gateron,"Gateron Blue Clicky (35 pcs)","gateron-blue-35pcs",280000,420), null, Av(kbswType!,kswClicky!), Av(kbswActu!,kswMed!));
            }

            // ── Gaming Mice ──────────────────────────────────────────────────
            AddMulti(P(catGamMice,logitech,"Logitech G Pro X Superlight 2","logitech-gpro-x-superlight2",3200000,280), null,
                (0m, new[] { Av(gamMouseDpi,dpi25k), Av(gamMouseConn,gmWireless), Av(gamMouseColor, gmColorBlack) }),
                (0m, new[] { Av(gamMouseDpi,dpi25k), Av(gamMouseConn,gmWireless), Av(gamMouseColor, gmColorWhite) }),
                (150000m, new[] { Av(gamMouseDpi,dpi25k), Av(gamMouseConn,gmWireless), Av(gamMouseColor, gmColorPink) })
            );
            Add(P(catGamMice,razer,"Razer DeathAdder V3 Pro Wireless","razer-deathadder-v3-pro",3800000,150), null,         Av(gamMouseDpi,dpi36k),Av(gamMouseConn,gmWireless),Av(gamMouseColor,gmColorBlack));
            Add(P(catGamMice,steelseries,"SteelSeries Rival 5 Wired","steelseries-rival-5",900000,490), null,               Av(gamMouseDpi,dpi16k),Av(gamMouseConn,gmWired),Av(gamMouseColor,gmColorBlack));
            Add(P(catGamMice,asus,"ASUS ROG Harpe Ace Wireless","asus-rog-harpe-ace",2800000,120), null,                    Av(gamMouseDpi,dpi36k),Av(gamMouseConn,gmWireless),Av(gamMouseColor,gmColorBlack));
            Add(P(catGamMice,razer,"Razer Viper V3 HyperSpeed Wireless","razer-viper-v3-hyperspeed",1400000,320), null,      Av(gamMouseDpi,dpi36k),Av(gamMouseConn,gmWireless),Av(gamMouseColor,gmColorBlack));
            Add(P(catGamMice,logitech,"Logitech G402 Hyperion Fury Wired","logitech-g402",750000,600), null,                Av(gamMouseDpi,dpi16k),Av(gamMouseConn,gmWired),Av(gamMouseColor,gmColorBlack));

            // ── Office Mice ──────────────────────────────────────────────────
            if (catOffMice != null && omWired != null)
            {
                Add(P(catOffMice,logitech,"Logitech MX Master 3S","logitech-mx-master-3s",2400000,350), null,           Av(offMouseConn!,omWireless!),Av(offMouseSensor!,omOptical!));
                Add(P(catOffMice,logitech,"Logitech M720 Triathlon Wireless","logitech-m720",1400000,420), null,         Av(offMouseConn!,omWireless!),Av(offMouseSensor!,omOptical!));
                Add(P(catOffMice,logitech,"Logitech M100 Wired Optical","logitech-m100",250000,1200), null,              Av(offMouseConn!,omWired!),   Av(offMouseSensor!,omOptical!));
            }

            // ── Mouse Pads ───────────────────────────────────────────────────
            if (catMousePad != null && padLarge != null)
            {
                Add(P(catMousePad,steelseries,"SteelSeries QcK Heavy XXL","steelseries-qck-heavy-xxl",680000,780), null,  Av(padSize!,padXl!),   Av(padMat!,padCloth!));
                Add(P(catMousePad,razer,"Razer Gigantus V2 3XL","razer-gigantus-v2-3xl",750000,520), null,               Av(padSize!,padXl!),   Av(padMat!,padCloth!));
                Add(P(catMousePad,logitech,"Logitech G840 XL Cloth","logitech-g840-xl",850000,430), null,                Av(padSize!,padXl!),   Av(padMat!,padCloth!));
                Add(P(catMousePad,corsair,"Corsair MM350 Pro Large","corsair-mm350-pro-large",650000,380), null,          Av(padSize!,padLarge!),Av(padMat!,padCloth!));
                Add(P(catMousePad,steelseries,"SteelSeries QcK Hard XL","steelseries-qck-hard-xl",890000,210), null,     Av(padSize!,padXl!),   Av(padMat!,padHard!));
            }

            // ── Gaming Headsets ──────────────────────────────────────────────
            Add(P(catGamHS,corsair,"Corsair Virtuoso RGB Wireless XT","corsair-virtuoso-rgb-xt",4500000,95), null,              Av(gamHsConn,ghsWireless),Av(gamHsSurr,ghs71));
            Add(P(catGamHS,steelseries,"SteelSeries Arctis Nova Pro Wireless","steelseries-arctis-nova-pro-w",5800000,60), null, Av(gamHsConn,ghsWireless),Av(gamHsSurr,ghsStereo));
            Add(P(catGamHS,razer,"Razer BlackShark V2 Pro Wireless","razer-blackshark-v2-pro-w",3800000,130), null,             Av(gamHsConn,ghsWireless),Av(gamHsSurr,ghs71));
            Add(P(catGamHS,hyperx,"HyperX Cloud III Wired","hyperx-cloud-iii",1500000,380), null,                               Av(gamHsConn,ghsWired),   Av(gamHsSurr,ghs71));
            Add(P(catGamHS,asus,"ASUS ROG Delta S Wired","asus-rog-delta-s",2800000,120), null,                                 Av(gamHsConn,ghsWired),   Av(gamHsSurr,ghs71));

            // ── Wireless Headphones ──────────────────────────────────────────
            if (catWirelessHP != null && wpAncYes != null)
            {
                Add(P(catWirelessHP,sony,"Sony WH-1000XM5 Wireless ANC","sony-wh1000xm5",8500000,200), null,           Av(wirHpAnc!,wpAncYes!),Av(wirHpDriver!,wpD40!));
                Add(P(catWirelessHP,sennheiser,"Sennheiser Momentum 4 Wireless","sennheiser-momentum-4",7200000,85), null, Av(wirHpAnc!,wpAncYes!),Av(wirHpDriver!,wpD40!));
                Add(P(catWirelessHP,logitech,"Logitech G535 Wireless Headset","logitech-g535",2100000,170), null,       Av(wirHpAnc!,wpAncNo!), Av(wirHpDriver!,wpD40!));
                Add(P(catWirelessHP,razer,"Razer Opus X Wireless ANC","razer-opus-x",1900000,240), null,               Av(wirHpAnc!,wpAncYes!),Av(wirHpDriver!,wpD30!));
            }

            // ── Microphones ──────────────────────────────────────────────────
            if (catMic != null && micUsb != null)
            {
                Add(P(catMic,blueMic,"Blue Yeti USB Microphone","blue-yeti-usb",2800000,240), null,              Av(micType!,micUsb!),Av(micPattern!,micCardioid!));
                Add(P(catMic,elgato,"Elgato Wave:3 USB Condenser","elgato-wave3",2400000,180), null,             Av(micType!,micUsb!),Av(micPattern!,micCardioid!));
                Add(P(catMic,razer,"Razer Seiren V2 Pro","razer-seiren-v2-pro",2900000,95), null, Av(micType!,micUsb!),Av(micPattern!,micCardioid!));
                Add(P(catMic,blueMic,"Blue Yeti X USB Professional","blue-yeti-x",4200000,110), null, Av(micType!,micUsb!),Av(micPattern!,micOmni!));
            }
            
            // ── Spec Mapping for Migration ──────────────────────────────────
            var specMapping = new Dictionary<(int catId, string key), CategoryAttribute>();
            void Map(CategoryAttribute a, string key) => specMapping[(a.CategoryId, key)] = a;

            Map(gpuCuda, "CUDA Cores"); Map(gpuClock, "Boost Clock"); Map(gpuTdp, "TDP"); Map(gpuNode, "Process Node");
            Map(cpuThreads, "Threads"); Map(cpuBaseClock, "Base Clock"); Map(cpuBoostClock, "Max Boost Clock");
            Map(cpuCache, "L3 Cache"); Map(cpuTdp, "TDP");

            // ── Save Products & Variants ─────────────────────────────────────
            foreach (var (product, variants) in products)
            {
                // Parse SpecsJson and create ProductAttributeValues
                if (!string.IsNullOrEmpty(product.SpecsJson) && product.SpecsJson != "{}")
                {
                    try {
                        var specs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecsJson);
                        if (specs != null) {
                            foreach (var spec in specs) {
                                if (specMapping.TryGetValue((product.CategoryId, spec.Key), out var attr)) {
                                    product.AttributeValues.Add(new ProductAttributeValue {
                                        Id = Guid.NewGuid(),
                                        ProductId = product.Id,
                                        CategoryAttributeId = attr.Id,
                                        Value = spec.Value
                                    });
                                }
                            }
                        }
                    } catch { /* skip malformed */ }
                }

                int vIdx = 1;
                foreach (var (priceOffset, avList) in variants)
                {
                    var variant = new ProductVariant
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