using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewPagedResult> GetReviewsByProductIdAsync(Guid productId, int page = 1, int pageSize = 5, string sort = "newest", int? filterRating = null)
        {
            var query = _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .AsNoTracking();

            // Filter by rating
            if (filterRating.HasValue && filterRating.Value >= 1 && filterRating.Value <= 5)
            {
                query = query.Where(r => r.Rating == filterRating.Value);
            }

            // Sort
            query = sort switch
            {
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "highest" => query.OrderByDescending(r => r.Rating),
                "lowest" => query.OrderBy(r => r.Rating),
                _ => query.OrderByDescending(r => r.CreatedAt) // newest
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToReviewViewModel())
                .ToListAsync();

            var summary = await GetProductRatingSummaryAsync(productId);

            return new ReviewPagedResult
            {
                Reviews = reviews,
                Summary = summary,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public async Task<ReviewSummaryViewModel> GetProductRatingSummaryAsync(Guid productId)
        {
            var query = _context.Reviews
                .Where(r => r.ProductId == productId)
                .AsNoTracking();

            var totalReviews = await query.CountAsync();

            if (totalReviews == 0)
            {
                return new ReviewSummaryViewModel();
            }

            var averageRating = await query.AverageAsync(r => r.Rating);

            var distribution = new int[5];
            var groups = await query
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var g in groups)
            {
                if (g.Rating >= 1 && g.Rating <= 5)
                    distribution[g.Rating - 1] = g.Count;
            }

            return new ReviewSummaryViewModel
            {
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingDistribution = distribution
            };
        }

        public async Task<(bool Success, string Message)> CreateReviewAsync(string userId, CreateReviewModel model)
        {
            if (model.Rating < 1 || model.Rating > 5)
                return (false, "Rating phải từ 1 đến 5 sao.");

            // Check if user already reviewed this product
            var existing = await _context.Reviews
                .AnyAsync(r => r.ProductId == model.ProductId && r.UserId == userId);

            if (existing)
                return (false, "Bạn đã đánh giá sản phẩm này rồi.");

            // Check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == model.ProductId && !p.IsDeleted);
            if (!productExists)
                return (false, "Sản phẩm không tồn tại.");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ProductId = model.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Title = model.Title,
                Comment = model.Comment,
                IsVerifiedPurchase = false, // TODO: check order history
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return (true, "Đánh giá đã được gửi thành công!");
        }

        public async Task<(bool Success, string Message)> UpdateReviewAsync(Guid reviewId, string userId, int rating, string? title, string? comment)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return (false, "Không tìm thấy đánh giá.");

            if (review.UserId != userId)
                return (false, "Bạn không có quyền sửa đánh giá này.");

            if (rating < 1 || rating > 5)
                return (false, "Rating phải từ 1 đến 5 sao.");

            review.Rating = rating;
            review.Title = title;
            review.Comment = comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Đánh giá đã được cập nhật!");
        }

        public async Task<(bool Success, string Message)> DeleteReviewAsync(Guid reviewId, string userId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return (false, "Không tìm thấy đánh giá.");

            if (review.UserId != userId)
                return (false, "Bạn không có quyền xoá đánh giá này.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return (true, "Đánh giá đã được xoá!");
        }

        public async Task<List<ReviewViewModel>> GetUserReviewsAsync(string userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .Select(MapToReviewViewModel())
                .ToListAsync();
        }

        // ========== Private Mapping ==========

        /// <summary>
        /// Expression chung map Review → ReviewViewModel.
        /// ProductName tự có khi query Include(Product), null khi không Include.
        /// </summary>
        private static System.Linq.Expressions.Expression<Func<Review, ReviewViewModel>> MapToReviewViewModel()
        {
            return r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User.FullName ?? r.User.UserName ?? "Ẩn danh",
                ProductName = r.Product != null ? r.Product.Name : null,
                UserAvatar = r.User.AvatarUrl,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt
            };
        }
    }
}
