using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductDetailModel(ICatalogService catalogService, IReviewService reviewService, UserManager<ApplicationUser> userManager)
        {
            _catalogService = catalogService;
            _reviewService = reviewService;
            _userManager = userManager;
        }

        public ProductDetailViewModel Product { get; set; } = null!;
        public ReviewPagedResult ReviewResult { get; set; } = new();

        [BindProperty]
        public CreateReviewModel ReviewInput { get; set; } = new();

        public string? ReviewMessage { get; set; }
        public bool ReviewSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug, int reviewPage = 1, string sort = "newest")
        {
            if (string.IsNullOrWhiteSpace(slug))
                return RedirectToPage("/Index");

            var product = await _catalogService.GetProductDetailBySlugAsync(slug);

            if (product == null)
                return NotFound();

            Product = product;
            ReviewResult = await _reviewService.GetReviewsByProductIdAsync(product.Id, reviewPage, 5, sort);

            // Update product rating from reviews
            Product.AverageRating = ReviewResult.Summary.AverageRating;
            Product.ReviewCount = ReviewResult.Summary.TotalReviews;

            return Page();
        }

        public async Task<IActionResult> OnPostReviewAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return RedirectToPage("/Index");

            var product = await _catalogService.GetProductDetailBySlugAsync(slug);
            if (product == null)
                return NotFound();

            Product = product;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ReviewMessage = "Bạn cần đăng nhập để đánh giá.";
                ReviewSuccess = false;
                ReviewResult = await _reviewService.GetReviewsByProductIdAsync(product.Id);
                return Page();
            }

            ReviewInput.ProductId = product.Id;
            var (success, message) = await _reviewService.CreateReviewAsync(user.Id, ReviewInput);
            ReviewMessage = message;
            ReviewSuccess = success;

            ReviewResult = await _reviewService.GetReviewsByProductIdAsync(product.Id);
            Product.AverageRating = ReviewResult.Summary.AverageRating;
            Product.ReviewCount = ReviewResult.Summary.TotalReviews;

            return Page();
        }

        public async Task<IActionResult> OnGetReviewsJsonAsync(Guid productId, int reviewPage = 1, string sort = "newest", int? filterRating = null)
        {
            var result = await _reviewService.GetReviewsByProductIdAsync(productId, reviewPage, 5, sort, filterRating);
            return new JsonResult(new
            {
                reviews = result.Reviews.Select(r => new
                {
                    userName = r.UserName,
                    rating = r.Rating,
                    title = r.Title,
                    comment = r.Comment,
                    isVerifiedPurchase = r.IsVerifiedPurchase,
                    createdAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                }),
                totalPages = result.TotalPages,
                currentPage = result.CurrentPage,
                summary = new
                {
                    averageRating = result.Summary.AverageRating,
                    totalReviews = result.Summary.TotalReviews,
                    ratingDistribution = result.Summary.RatingDistribution
                }
            });
        }
    }
}
