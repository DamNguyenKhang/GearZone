using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.ProductSpecifications;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreFollowRepository _storeFollowRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CatalogService(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ICategoryAttributeRepository categoryAttributeRepository,
            IStoreRepository storeRepository,
            IStoreFollowRepository storeFollowRepository,
            IConversationRepository conversationRepository,
            IChatMessageRepository chatMessageRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _categoryAttributeRepository = categoryAttributeRepository;
            _storeRepository = storeRepository;
            _storeFollowRepository = storeFollowRepository;
            _conversationRepository = conversationRepository;
            _chatMessageRepository = chatMessageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CatalogProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            return await _productRepository.GetFilteredProductsAsync(filter);
        }

        public async Task<StoreProfileDto?> GetStoreProfileAsync(string slug, string? currentUserId = null)
        {
            var store = await _storeRepository.GetBySlugAsync(slug);
            if (store == null) return null;

            var productCount = await _productRepository.Query()
                .CountAsync(p => p.StoreId == store.Id && !p.IsDeleted 
                    && p.Status == GearZone.Domain.Enums.ProductStatus.Active);

            var totalSold = await _productRepository.Query()
                .Where(p => p.StoreId == store.Id && !p.IsDeleted)
                .SumAsync(p => p.SoldCount);

            var followerCount = await _storeFollowRepository.GetFollowerCountAsync(store.Id);
            var isFollowing = !string.IsNullOrEmpty(currentUserId) 
                && await _storeFollowRepository.ExistsAsync(currentUserId, store.Id);

            return new StoreProfileDto
            {
                Id = store.Id,
                StoreName = store.StoreName,
                Slug = store.Slug,
                Description = store.Description,
                LogoUrl = store.LogoUrl,
                Province = store.Province,
                ProductCount = productCount,
                TotalSold = totalSold,
                Rating = 0,
                ReviewCount = 0,
                FollowerCount = followerCount,
                IsFollowing = isFollowing,
                CreatedAt = store.CreatedAt
            };
        }

        // ===== FOLLOW =====

        public async Task<bool> ToggleFollowAsync(string userId, Guid storeId)
        {
            var existing = await _storeFollowRepository.GetByUserAndStoreAsync(userId, storeId);
            if (existing != null)
            {
                await _storeFollowRepository.DeleteAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                return false; // unfollowed
            }

            var follow = new StoreFollow
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                FollowedAt = DateTime.UtcNow
            };
            await _storeFollowRepository.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return true; // followed
        }

        public async Task<bool> IsFollowingAsync(string userId, Guid storeId)
        {
            return await _storeFollowRepository.ExistsAsync(userId, storeId);
        }

        public async Task<int> GetFollowerCountAsync(Guid storeId)
        {
            return await _storeFollowRepository.GetFollowerCountAsync(storeId);
        }

        // ===== CHAT =====

        public async Task<ChatMessageDto> SendMessageAsync(string userId, Guid storeId, string content)
        {
            var conversation = await _conversationRepository.GetByBuyerAndStoreAsync(userId, storeId);
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    BuyerUserId = userId,
                    StoreId = storeId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                await _conversationRepository.AddAsync(conversation);
            }
            else
            {
                conversation.LastMessageAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderUserId = userId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            await _chatMessageRepository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return new ChatMessageDto
            {
                Id = message.Id,
                SenderUserId = message.SenderUserId,
                SenderName = "You",
                Content = message.Content,
                SentAt = message.SentAt,
                IsFromStore = false
            };
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(string userId, Guid storeId, int page = 1, int pageSize = 50)
        {
            var conversation = await _conversationRepository.GetByBuyerAndStoreAsync(userId, storeId);
            if (conversation == null) return new List<ChatMessageDto>();

            // Get store owner to determine which messages are from store
            var store = await _storeRepository.GetByIdAsync(storeId);
            var storeOwnerId = store?.OwnerUserId;

            var messages = await _chatMessageRepository.GetMessagesAsync(conversation.Id, page, pageSize);
            
            return messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderUser?.FullName ?? "User",
                SenderAvatar = m.SenderUser?.AvatarUrl,
                Content = m.Content,
                SentAt = m.SentAt,
                IsFromStore = m.SenderUserId == storeOwnerId
            }).ToList();
        }

        // ===== CATEGORIES & FILTERS =====

        public async Task<List<CatalogCategoryDto>> GetCategoriesAsync()
        {
            return await _categoryRepository.Query()
                .Where(c => c.IsActive && !c.IsDeleted && c.ParentId == null)
                .Select(c => new CatalogCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    SubCategories = c.Children
                        .Where(child => child.IsActive && !child.IsDeleted)
                        .Select(child => new CatalogCategoryDto
                        {
                            Id = child.Id,
                            Name = child.Name,
                            Slug = child.Slug
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<CatalogFilterSidebarDto> GetFiltersForCategoryAsync(string categorySlug)
        {
            var result = new CatalogFilterSidebarDto();

            // Find category and eagerly check if it has children
            var category = await _categoryRepository.Query()
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive);

            if (category == null) return result;

            bool isParent = category.Children != null && category.Children.Any(c => c.IsActive && !c.IsDeleted);

            if (isParent)
            {
                // -- PARENT CATEGORY: aggregate brands from all active children --
                var childIds = category.Children!
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .Select(c => c.Id)
                    .ToList();

                // Include the parent itself in case it has direct products
                childIds.Add(category.Id);

                result.Brands = await _productRepository.Query()
                    .Where(p => childIds.Contains(p.CategoryId)
                              && p.Brand.IsApproved
                              && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                              && !p.IsDeleted)
                    .GroupBy(p => p.Brand)
                    .Select(g => new BrandFilterDto
                    {
                        Name = g.Key.Name,
                        Slug = g.Key.Slug,
                        ProductCount = g.Count()
                    })
                    .OrderByDescending(b => b.ProductCount)
                    .ToListAsync();

                // Do NOT return dynamic attributes for parent categories:
                // each subcategory has its own unrelated attributes (VRAM vs Socket vs Panel Type…)
                // Filters shown: Brand + Price range only (Price is always rendered on frontend)
            }
            else
            {
                // -- LEAF CATEGORY: original behavior — brands + dynamic attributes --
                result.Brands = await _productRepository.Query()
                    .Where(p => p.CategoryId == category.Id
                              && p.Brand.IsApproved
                              && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                              && !p.IsDeleted)
                    .GroupBy(p => p.Brand)
                    .Select(g => new BrandFilterDto
                    {
                        Name = g.Key.Name,
                        Slug = g.Key.Slug,
                        ProductCount = g.Count()
                    })
                    .ToListAsync();

                var attributes = await _categoryAttributeRepository.Query()
                    .Where(a => a.CategoryId == category.Id && a.IsFilterable && a.Scope != GearZone.Domain.Enums.AttributeScope.Product && a.Options.Any())
                    .OrderBy(a => a.DisplayOrder)
                    .ToListAsync();

                foreach (var attr in attributes)
                {
                    var attrDto = new CategoryAttributeFilterDto
                    {
                        Name = attr.Name,
                        FilterType = attr.FilterType
                    };

                    attrDto.Values = await _productRepository.Query()
                        .Where(p => p.CategoryId == category.Id
                                  && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                                  && !p.IsDeleted)
                        .SelectMany(p => p.Variants)
                        .SelectMany(v => v.AttributeValues)
                        .Where(av => av.CategoryAttributeId == attr.Id)
                        .GroupBy(av => av.CategoryAttributeOption)
                        .Select(g => new AttributeValueFilterDto
                        {
                            Value = g.Key.Value,
                            ProductCount = g.Select(x => x.Variant.ProductId).Distinct().Count()
                        })
                        .OrderBy(x => x.Value)
                        .ToListAsync();

                    if (attrDto.Values.Any())
                        result.Attributes.Add(attrDto);
                }
            }

            return result;
        }

        public async Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug)
        {
            var product = await _productRepository.Query()
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted);

            if (product == null) return null;

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                BasePrice = product.BasePrice,
                SoldCount = product.SoldCount,
                BrandName = product.Brand.Name,
                BrandSlug = product.Brand.Slug,
                CategoryName = product.Category.Name,
                CategorySlug = product.Category.Slug,
                StoreId = product.StoreId,
                StoreName = product.Store.StoreName,
                StoreSlug = product.Store.Slug,
                ImageUrls = product.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).ToList(),
            };

            var allAttributeValues = product.Variants.SelectMany(v => v.AttributeValues).ToList();

            if (product.Variants.Any())
            {
                dto.AttributeSelections = allAttributeValues
                    .GroupBy(av => av.CategoryAttributeId)
                    .Select(g => new AttributeSelectionDto
                    {
                        AttributeId = g.Key,
                        Name = g.First().CategoryAttribute.Name,
                        Options = g.Select(av => av.CategoryAttributeOption)
                                   .DistinctBy(opt => opt.Id)
                                   .Select(opt => new AttributeOptionDto
                                   {
                                       OptionId = opt.Id,
                                       Value = opt.Value
                                   }).ToList()
                    }).ToList();

                dto.Variants = product.Variants.Select(v => new VariantDetailDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    SelectedOptionIds = v.AttributeValues.Select(av => av.CategoryAttributeOptionId).ToList()
                }).ToList();
            }

            var legacySpecs = ParseSpecificationsJson(product.SpecsJson);
            var specs = new List<SpecificationDto>();

            var structuredSpecs = product.AttributeValues
                .Where(av => av.CategoryAttribute != null)
                .OrderBy(av => av.CategoryAttribute.DisplayOrder)
                .ThenBy(av => av.CategoryAttribute.Name)
                .Select(av => new SpecificationDto
                {
                    Name = av.CategoryAttribute.Name,
                    Value = FormatSpecificationValue(av.CategoryAttributeOption?.Value ?? av.Value ?? string.Empty, av.CategoryAttribute.Unit)
                })
                .Where(spec => !string.IsNullOrWhiteSpace(spec.Value))
                .ToList();

            specs.AddRange(structuredSpecs);

            foreach (var definition in SeededProductSpecificationCatalog.GetTemplate(product.Category.Slug))
            {
                if (specs.Any(spec => spec.Name.Equals(definition.DisplayName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var legacyValue = SeededProductSpecificationCatalog.FindLegacyValue(product.Category.Slug, legacySpecs, definition.DisplayName);
                if (!string.IsNullOrWhiteSpace(legacyValue))
                {
                    specs.Add(new SpecificationDto
                    {
                        Name = definition.DisplayName,
                        Value = legacyValue
                    });
                }
            }

            foreach (var legacySpec in legacySpecs)
            {
                if (specs.Any(spec => spec.Name.Equals(legacySpec.Key, StringComparison.OrdinalIgnoreCase))
                    || SeededProductSpecificationCatalog.GetDefinition(product.Category.Slug, legacySpec.Key) != null)
                {
                    continue;
                }

                specs.Add(new SpecificationDto
                {
                    Name = legacySpec.Key,
                    Value = legacySpec.Value
                });
            }

            var existingNames = new HashSet<string>(specs.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
            var variantAttributeSpecs = allAttributeValues
                .GroupBy(av => av.CategoryAttribute.Name)
                .Where(g => !existingNames.Contains(g.Key))
                .Select(g => new SpecificationDto
                {
                    Name = g.Key,
                    Value = string.Join(", ", g.Select(av => av.CategoryAttributeOption.Value).Distinct())
                })
                .ToList();

            specs.AddRange(variantAttributeSpecs);
            dto.Specifications = specs;

            return dto;
        }

        public async Task<List<CatalogProductDto>> GetRelatedProductsAsync(int categoryId, Guid currentProductId, int limit = 5)
        {
            var relatedProducts = await _productRepository.Query()
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId 
                         && p.Id != currentProductId 
                         && p.Status == GearZone.Domain.Enums.ProductStatus.Active 
                         && !p.IsDeleted)
                .OrderByDescending(p => p.SoldCount) // Best selling in the same category
                .Take(limit)
                .Select(p => new CatalogProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() 
                               ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                    Rating = 0, // Placeholder
                    ReviewCount = 0,
                    StoreName = p.Store.StoreName,
                    StoreLogoUrl = p.Store.LogoUrl ?? string.Empty,
                    IsInStock = p.Variants.Where(v => v.IsActive && !v.IsDeleted).Any(v => v.StockQuantity > 0),
                    DefaultVariantId = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .Select(v => v.Id)
                        .FirstOrDefault(),
                    HighlightTags = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();

            return relatedProducts;
        }

        public async Task<List<ProductSuggestionDto>> GetProductSuggestionsAsync(string query, int limit = 5)
        {
            return await _productRepository.GetProductSuggestionsAsync(query, limit);
        }

        public async Task<ProductComparisonDto?> GetProductComparisonAsync(int categoryId, List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any()) return null;

            // If categoryId is missing, try to infer it from the first product
            if (categoryId <= 0)
            {
                var firstProduct = await _productRepository.Query()
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .FirstOrDefaultAsync();
                
                if (firstProduct == null) return null;
                categoryId = firstProduct.CategoryId;
            }

            var products = await _productRepository.Query()
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttributeOption)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(p => productIds.Contains(p.Id) && p.CategoryId == categoryId)
                .ToListAsync();

            if (!products.Any()) return null;

            // Sort products to match the request order
            var sortedProducts = productIds
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Cast<Product>()
                .ToList();

            var result = new ProductComparisonDto();
            result.Products = sortedProducts.Select(p => new ComparisonHeaderDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Brand = p.Brand.Name,
                Price = p.BasePrice,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl ?? ""
            }).ToList();
            var categorySlug = await _categoryRepository.Query()
                .Where(c => c.Id == categoryId)
                .Select(c => c.Slug)
                .FirstOrDefaultAsync();

            var comparableAttributes = await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == categoryId && a.IsComparable)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            if (!comparableAttributes.Any())
            {
                comparableAttributes = await _categoryAttributeRepository.Query()
                    .Where(a => a.CategoryId == categoryId)
                    .OrderBy(a => a.DisplayOrder)
                    .ToListAsync();
            }

            var comparisonDefinitions = comparableAttributes
                .Select(attr => new ComparisonDefinition
                {
                    AttributeId = attr.Id,
                    AttributeName = attr.Name,
                    Unit = attr.Unit
                })
                .ToList();

            var legacySpecsByProductId = sortedProducts.ToDictionary(p => p.Id, p => ParseSpecificationsJson(p.SpecsJson));

            foreach (var definition in SeededProductSpecificationCatalog.GetTemplate(categorySlug))
            {
                if (comparisonDefinitions.Any(item => item.AttributeName.Equals(definition.DisplayName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                comparisonDefinitions.Add(new ComparisonDefinition
                {
                    AttributeId = 0,
                    AttributeName = definition.DisplayName,
                    Unit = definition.Unit
                });
            }

            foreach (var product in sortedProducts)
            {
                foreach (var attributeValue in product.AttributeValues.Where(av => av.CategoryAttribute != null))
                {
                    if (comparisonDefinitions.Any(item =>
                        item.AttributeId == attributeValue.CategoryAttributeId
                        || item.AttributeName.Equals(attributeValue.CategoryAttribute.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    comparisonDefinitions.Add(new ComparisonDefinition
                    {
                        AttributeId = attributeValue.CategoryAttributeId,
                        AttributeName = attributeValue.CategoryAttribute.Name,
                        Unit = attributeValue.CategoryAttribute.Unit
                    });
                }

                foreach (var attributeValue in product.Variants
                    .SelectMany(v => v.AttributeValues)
                    .Where(av => av.CategoryAttribute != null))
                {
                    if (comparisonDefinitions.Any(item =>
                        item.AttributeId == attributeValue.CategoryAttributeId
                        || item.AttributeName.Equals(attributeValue.CategoryAttribute.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    comparisonDefinitions.Add(new ComparisonDefinition
                    {
                        AttributeId = attributeValue.CategoryAttributeId,
                        AttributeName = attributeValue.CategoryAttribute.Name,
                        Unit = attributeValue.CategoryAttribute.Unit
                    });
                }
            }

            foreach (var legacySpecs in legacySpecsByProductId.Values)
            {
                foreach (var legacySpec in legacySpecs)
                {
                    if (comparisonDefinitions.Any(item => item.AttributeName.Equals(legacySpec.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    comparisonDefinitions.Add(new ComparisonDefinition
                    {
                        AttributeId = 0,
                        AttributeName = legacySpec.Key
                    });
                }
            }

            foreach (var definition in comparisonDefinitions)
            {
                var row = new ComparisonRowDto
                {
                    AttributeId = definition.AttributeId,
                    AttributeName = definition.AttributeName,
                    Unit = definition.Unit,
                    Values = new List<string?>()
                };

                foreach (var product in sortedProducts)
                {
                    var pieces = new List<string>();

                    if (definition.AttributeId > 0)
                    {
                        var productVal = product.AttributeValues.FirstOrDefault(av => av.CategoryAttributeId == definition.AttributeId);
                        if (productVal != null)
                        {
                            var productDisplay = FormatSpecificationValue(productVal.CategoryAttributeOption?.Value ?? productVal.Value ?? string.Empty, definition.Unit);
                            if (!string.IsNullOrWhiteSpace(productDisplay))
                            {
                                pieces.Add(productDisplay);
                            }
                        }

                        var variantVals = product.Variants
                            .SelectMany(v => v.AttributeValues)
                            .Where(av => av.CategoryAttributeId == definition.AttributeId)
                            .Select(av => av.CategoryAttributeOption?.Value)
                            .Where(value => !string.IsNullOrWhiteSpace(value))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Cast<string>()
                            .ToList();

                        if (variantVals.Any())
                        {
                            pieces.AddRange(variantVals);
                        }
                    }

                    var displayValue = pieces
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var merged = displayValue.Any() ? string.Join(", ", displayValue) : null;

                    if (string.IsNullOrWhiteSpace(merged))
                    {
                        merged = SeededProductSpecificationCatalog.FindLegacyValue(categorySlug, legacySpecsByProductId[product.Id], definition.AttributeName);
                    }

                    row.Values.Add(string.IsNullOrWhiteSpace(merged) ? null : merged);
                }



                var distinctValues = row.Values
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                row.IsDifferent = distinctValues.Count > 1;

                result.Rows.Add(row);
            }

            return result;
        }

        private static Dictionary<string, string> ParseSpecificationsJson(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "{}")
            {
                return new Dictionary<string, string>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(rawJson) ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private static string FormatSpecificationValue(string rawValue, string? unit)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(unit) || rawValue.Trim().EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                return rawValue.Trim();
            }

            return $"{rawValue.Trim()} {unit}";
        }

        private sealed class ComparisonDefinition
        {
            public int AttributeId { get; set; }
            public string AttributeName { get; set; } = string.Empty;
            public string? Unit { get; set; }
        }
    }
}












