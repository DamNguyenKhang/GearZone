using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

            var specs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(product.SpecsJson))
            {
                try
                {
                    specs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecsJson) ?? new();
                }
                catch { /* Ignore invalid JSON */ }
            }

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
                Specifications = specs,
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

            // 2. Handle Images (Simulated path for now, usually handled by a separate storage service)
            if (dto.Images != null && dto.Images.Any())
            {
                int sortOrder = 0;
                foreach (var file in dto.Images.Take(5))
                {
                    // In a real app, we'd save to disk/cloud here
                    // e.g., var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    // For this task, we'll assume they are saved and just store a dummy path or original name
                    var imageUrl = $"/uploads/products/{file.FileName}"; 
                    
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
    }
}
