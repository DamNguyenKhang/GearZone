using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    // ViewModels
    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? UserAvatar { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewSummaryViewModel
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int[] RatingDistribution { get; set; } = new int[5]; // index 0 = 1 star, index 4 = 5 stars
    }

    public class CreateReviewModel
    {
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
    }

    public class ReviewPagedResult
    {
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public ReviewSummaryViewModel Summary { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public interface IReviewService
    {
        Task<ReviewPagedResult> GetReviewsByProductIdAsync(Guid productId, int page = 1, int pageSize = 5, string sort = "newest", int? filterRating = null);
        Task<ReviewSummaryViewModel> GetProductRatingSummaryAsync(Guid productId);
        Task<(bool Success, string Message)> CreateReviewAsync(string userId, CreateReviewModel model);
        Task<(bool Success, string Message)> UpdateReviewAsync(Guid reviewId, string userId, int rating, string? title, string? comment);
        Task<(bool Success, string Message)> DeleteReviewAsync(Guid reviewId, string userId);
        Task<List<ReviewViewModel>> GetUserReviewsAsync(string userId);
    }
}
