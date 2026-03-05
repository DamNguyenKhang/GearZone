using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Persistence;
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
                .Include(c => c.Children)
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

            var category = await _categoryRepository.Query()
                .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive);

            if (category == null) return result;

            result.Brands = await _productRepository.Query()
                .Where(p => p.CategoryId == category.Id && p.Brand.IsApproved && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted)
                .GroupBy(p => p.Brand)
                .Select(g => new BrandFilterDto
                {
                    Name = g.Key.Name,
                    Slug = g.Key.Slug,
                    ProductCount = g.Count()
                })
                .ToListAsync();

            var attributes = await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == category.Id && a.IsFilterable)
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
                    .Where(p => p.CategoryId == category.Id && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted)
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
                {
                    result.Attributes.Add(attrDto);
                }
            }

            return result;
        }
    }
}
