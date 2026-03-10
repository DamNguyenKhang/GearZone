using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace GearZone.Application.Features.Seller
{
    public class SellerProductService : ISellerProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IInventoryTransactionRepository _inventoryRepository;
        private readonly IVariantAttributeValueRepository _attributeValueRepository;
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public SellerProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            IProductImageRepository productImageRepository,
            IProductVariantRepository productVariantRepository,
            IInventoryTransactionRepository inventoryRepository,
            IVariantAttributeValueRepository attributeValueRepository,
            ICategoryAttributeRepository categoryAttributeRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productImageRepository = productImageRepository;
            _productVariantRepository = productVariantRepository;
            _inventoryRepository = inventoryRepository;
            _attributeValueRepository = attributeValueRepository;
            _categoryAttributeRepository = categoryAttributeRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SellerProductListDto>> GetProductsByStoreAsync(Guid storeId)
        {
            return await _productRepository.Query()
                .Where(p => p.StoreId == storeId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SellerProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    TotalStock = p.Variants.Sum(v => v.StockQuantity),
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    PrimaryImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<SellerProductDetailDto?> GetProductByIdAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) return null;


            return new SellerProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                CategoryName = product.Category.Name,
                BrandName = product.Brand.Name,
                BasePrice = product.BasePrice,
                SoldCount = product.SoldCount,
                Status = product.Status.ToString(),
                CreatedAt = product.CreatedAt,
                Specifications = string.IsNullOrEmpty(product.SpecsJson) 
                    ? new Dictionary<string, string>() 
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecsJson) ?? new Dictionary<string, string>(),
                ImageUrls = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantDto
                {
                    VariantName = v.VariantName ?? "",
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.AttributeValues.Select(av => new AttributeSelectionDto
                    {
                        AttributeId = av.CategoryAttributeId,
                        OptionId = av.CategoryAttributeOptionId,
                        AttributeName = av.CategoryAttribute?.Name ?? "",
                        OptionValue = av.CategoryAttributeOption?.Value ?? ""
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<Guid> CreateProductAsync(CreateProductDto dto, Guid storeId, string userId)
        {
            // 0. Validation
            var slug = string.IsNullOrEmpty(dto.Slug) ? dto.Name.ToLower().Replace(" ", "-") : dto.Slug;
            
            var existingProduct = await _productRepository.Query()
                .AnyAsync(p => p.StoreId == storeId && p.Slug == slug && !p.IsDeleted);
            
            if (existingProduct)
            {
                throw new InvalidOperationException($"A product with the slug '{slug}' already exists in your store.");
            }

            if (dto.Variants != null && dto.Variants.Any())
            {
                var skus = dto.Variants.Select(v => v.Sku).ToList();
                var existingSkus = await _productVariantRepository.Query()
                    .Where(v => skus.Contains(v.Sku))
                    .Select(v => v.Sku)
                    .ToListAsync();
                
                if (existingSkus.Any())
                {
                    throw new InvalidOperationException($"The following SKUs already exist in the system: {string.Join(", ", existingSkus)}");
                }
            }

            // 1. Create Product
            var product = new Product
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Status = dto.IsDraft ? ProductStatus.Draft : ProductStatus.Active,
                SoldCount = 0,
                CreatedAt = DateTime.UtcNow,
                SpecsJson = JsonSerializer.Serialize(dto.Specifications?
                    .Where(s => !string.IsNullOrWhiteSpace(s.Key))
                    .ToDictionary(s => s.Key, s => s.Value) ?? new Dictionary<string, string>())
            };  

            await _productRepository.AddAsync(product);

            // 2. Handle Images (Cloudinary)
            if (dto.Images != null && dto.Images.Any())
            {
                var imageUrls = await _fileStorageService.UploadAsync(dto.Images);
                int sortOrder = 0;
                foreach (var imageUrl in imageUrls)
                {
                    await _productImageRepository.AddAsync(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = sortOrder == 0,
                        SortOrder = sortOrder++
                    });
                }
            }

            // 3. Handle Variants
            foreach (var vDto in dto.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = vDto.Sku,
                    VariantName = vDto.VariantName,
                    Price = vDto.Price,
                    StockQuantity = vDto.StockQuantity,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _productVariantRepository.AddAsync(variant);

                // 4. Initial Inventory Task
                if (variant.StockQuantity > 0)
                {
                    await _inventoryRepository.AddAsync(new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        VariantId = variant.Id,
                        Type = "StockIn",
                        QuantityChange = variant.StockQuantity,
                        Reason = "Initial stock upload",
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId
                    });
                }

                // 5. Dynamic Attributes for Variant (if any)
                foreach (var attr in vDto.Attributes)
                {
                    await _attributeValueRepository.AddAsync(new VariantAttributeValue
                    {
                        Id = Guid.NewGuid(),
                        VariantId = variant.Id,
                        CategoryAttributeId = attr.AttributeId,
                        CategoryAttributeOptionId = attr.OptionId
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return product.Id;
        }

        public async Task<UpdateProductDto?> GetProductForEditAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) return null;

            return new UpdateProductDto
            {
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                BasePrice = product.BasePrice,
                IsDraft = product.Status == ProductStatus.Draft,
                ExistingImageUrls = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                Variants = product.Variants.Where(v => !v.IsDeleted).Select(v => new ProductVariantDto
                {
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.AttributeValues.Select(av => new AttributeSelectionDto
                    {
                        AttributeId = av.CategoryAttributeId,
                        OptionId = av.CategoryAttributeOptionId
                    }).ToList()
                }).ToList()
            };
        }

        public async Task UpdateProductAsync(Guid productId, UpdateProductDto dto, Guid storeId, string userId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) throw new InvalidOperationException("Product not found.");

            // 1. Validation for Slug (if changed)
            var slug = string.IsNullOrEmpty(dto.Slug) ? dto.Name.ToLower().Replace(" ", "-") : dto.Slug;
            if (slug != product.Slug)
            {
                var existingSlug = await _productRepository.Query()
                    .AnyAsync(p => p.StoreId == storeId && p.Slug == slug && p.Id != productId && !p.IsDeleted);
                if (existingSlug) throw new InvalidOperationException($"Slug '{slug}' is already in use.");
            }

            // 2. Update Basic Info
            product.Name = dto.Name;
            product.Slug = slug;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.BasePrice = dto.BasePrice;
            product.Status = dto.IsDraft ? ProductStatus.Draft : ProductStatus.Active;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            // 3. Handle Images (Cloudinary)
            if (dto.NewImages != null && dto.NewImages.Any())
            {
                var currentImageCount = product.Images.Count;
                var imageUrls = await _fileStorageService.UploadAsync(dto.NewImages);
                int sortOrder = currentImageCount;

                foreach (var imageUrl in imageUrls)
                {
                    if (sortOrder >= 5) break;

                    await _productImageRepository.AddAsync(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = sortOrder == 0,
                        SortOrder = sortOrder++
                    });
                }
            }

            // 4. Handle Variants
            // Logic: 
            // - If SKU exists in Input, update it.
            // - If SKU doesn't exist in DB, add it.
            // - If SKU exists in DB but NOT in Input, delete it (soft delete).

            var existingVariants = product.Variants.ToList();
            var inputVariants = dto.Variants ?? new List<ProductVariantDto>();

            // Soft delete removed variants
            foreach (var ev in existingVariants.Where(v => !v.IsDeleted))
            {
                if (!inputVariants.Any(iv => iv.Sku == ev.Sku))
                {
                    ev.IsDeleted = true;
                    ev.UpdatedAt = DateTime.UtcNow;
                    await _productVariantRepository.UpdateAsync(ev);
                }
            }

            // Update or Add
            foreach (var iv in inputVariants)
            {
                var ev = existingVariants.FirstOrDefault(v => v.Sku == iv.Sku);
                if (ev != null)
                {
                    // Update existing
                    ev.VariantName = iv.VariantName;
                    ev.Price = iv.Price;
                    ev.IsActive = true;
                    ev.IsDeleted = false; // Re-active if it was deleted
                    ev.UpdatedAt = DateTime.UtcNow;

                    // Stock adjustment via transaction if changed
                    if (iv.StockQuantity != ev.StockQuantity)
                    {
                        var diff = iv.StockQuantity - ev.StockQuantity;
                        await _inventoryRepository.AddAsync(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            VariantId = ev.Id,
                            Type = diff > 0 ? "AdjustmentIn" : "AdjustmentOut",
                            QuantityChange = diff,
                            Reason = "Manual adjustment during product update",
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = userId
                        });
                        ev.StockQuantity = iv.StockQuantity;
                    }

                    await _productVariantRepository.UpdateAsync(ev);

                    // Attributes (Sync: remove existing and re-add for simplicity in this case)
                    // Note: In a large scale app, we'd do a more surgical update.
                    foreach (var attr in ev.AttributeValues.ToList())
                    {
                        await _attributeValueRepository.DeleteAsync(attr);
                    }

                    foreach (var attr in iv.Attributes)
                    {
                        await _attributeValueRepository.AddAsync(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            VariantId = ev.Id,
                            CategoryAttributeId = attr.AttributeId,
                            CategoryAttributeOptionId = attr.OptionId
                        });
                    }
                }
                else
                {
                    // Add new variant
                    var newVariant = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Sku = iv.Sku,
                        VariantName = iv.VariantName,
                        Price = iv.Price,
                        StockQuantity = iv.StockQuantity,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _productVariantRepository.AddAsync(newVariant);

                    if (newVariant.StockQuantity > 0)
                    {
                        await _inventoryRepository.AddAsync(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            VariantId = newVariant.Id,
                            Type = "StockIn",
                            QuantityChange = newVariant.StockQuantity,
                            Reason = "Initial stock for new variant",
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = userId
                        });
                    }

                    foreach (var attr in iv.Attributes)
                    {
                        await _attributeValueRepository.AddAsync(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            VariantId = newVariant.Id,
                            CategoryAttributeId = attr.AttributeId,
                            CategoryAttributeOptionId = attr.OptionId
                        });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.Query()
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync();
        }

        public async Task<List<Brand>> GetBrandsAsync()
        {
            return await _brandRepository.Query()
                .Where(b => b.IsApproved)
                .ToListAsync();
        }

        public async Task<List<CategoryAttributeDto>> GetCategoryAttributesAsync(int categoryId)
        {
            var attributes = await _categoryAttributeRepository.Query()
                .Include(a => a.Options)
                .Where(a => a.CategoryId == categoryId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            return attributes.Select(a => new CategoryAttributeDto
            {
                Id = a.Id,
                Name = a.Name,
                FilterType = a.FilterType,
                Options = a.Options.Select(o => new CategoryAttributeOptionDto
                {
                    Id = o.Id,
                    Value = o.Value
                }).ToList()
            }).ToList();
        }

        public async Task ToggleProductStatusAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) throw new InvalidOperationException("Product not found.");

            product.Status = product.Status == ProductStatus.Active ? ProductStatus.Inactive : ProductStatus.Active;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> CreateBrandByNameAsync(string name)
        {
            var slug = name.ToLower().Replace(" ", "-");
            var brand = new Brand
            {
                Name = name,
                Slug = slug,
                IsApproved = true, // Auto-approve for now, or keep as false if admin review is required
                CreatedAt = DateTime.UtcNow
            };

            await _brandRepository.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            return brand.Id;
        }

        public async Task<int> CreateCategoryByNameAsync(string name)
        {
            var slug = name.ToLower().Replace(" ", "-");
            var category = new Category
            {
                Name = name,
                Slug = slug,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.Id;
        }
    }
}
