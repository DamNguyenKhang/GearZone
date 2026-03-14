using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Domain.Entities;
using GearZone.Application.Abstractions.Services;

namespace GearZone.Web.Pages.Public.StoreProfile
{
    public class DetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ICatalogService catalogService, UserManager<ApplicationUser> userManager)
        {
            _catalogService = catalogService;
            _userManager = userManager;
        }

        public StoreProfileDto Store { get; set; } = null!;
        public PagedResult<CatalogProductDto> Products { get; set; } = null!;
        public List<CatalogCategoryDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? CategorySlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var storeProfile = await _catalogService.GetStoreProfileAsync(slug, currentUserId);
            if (storeProfile == null)
                return NotFound();

            Store = storeProfile;

            var filter = new ProductFilterDto
            {
                StoreId = Store.Id,
                CategorySlug = CategorySlug,
                SortBy = SortBy ?? "popular",
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                PageNumber = PageNumber,
                PageSize = 20
            };

            Products = await _catalogService.GetProductsAsync(filter);
            Categories = await _catalogService.GetCategoriesAsync();

            return Page();
        }

        // AJAX: Toggle Follow
        public async Task<IActionResult> OnPostFollowAsync(string slug)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Please login first" }) { StatusCode = 401 };

            var store = await _catalogService.GetStoreProfileAsync(slug);
            if (store == null)
                return new JsonResult(new { success = false, message = "Store not found" }) { StatusCode = 404 };

            var isFollowing = await _catalogService.ToggleFollowAsync(userId, store.Id);
            var followerCount = await _catalogService.GetFollowerCountAsync(store.Id);

            return new JsonResult(new { success = true, isFollowing, followerCount });
        }

        // AJAX: Send Message
        public async Task<IActionResult> OnPostSendMessageAsync(string slug, [FromBody] SendMessageRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Please login first" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(request?.Content))
                return new JsonResult(new { success = false, message = "Message cannot be empty" }) { StatusCode = 400 };

            var store = await _catalogService.GetStoreProfileAsync(slug);
            if (store == null)
                return new JsonResult(new { success = false, message = "Store not found" }) { StatusCode = 404 };

            var message = await _catalogService.SendMessageAsync(userId, store.Id, request.Content.Trim());

            return new JsonResult(new { success = true, message });
        }

        // AJAX: Get Messages
        public async Task<IActionResult> OnGetMessagesAsync(string slug)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, messages = new List<ChatMessageDto>() });

            var store = await _catalogService.GetStoreProfileAsync(slug);
            if (store == null)
                return new JsonResult(new { success = false, messages = new List<ChatMessageDto>() });

            var messages = await _catalogService.GetMessagesAsync(userId, store.Id);

            return new JsonResult(new { success = true, messages });
        }
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
