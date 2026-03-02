using System;

namespace GearZone.Web.Pages.Shared.Models
{
    public class ProductCardViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string PrimaryImageUrl { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Format giá theo định dạng VNĐ: 24.990.000 ₫
        /// </summary>
        public string FormattedPrice => MinPrice.ToString("#,0").Replace(",", ".") + " ₫";

        /// <summary>
        /// Format giá gốc (nếu có)
        /// </summary>
        public string? FormattedOriginalPrice => OriginalPrice?.ToString("#,0").Replace(",", ".") + " ₫";

        /// <summary>
        /// Tính số sao filled (1-5)
        /// </summary>
        public int FullStars => (int)Math.Floor(AverageRating);

        /// <summary>
        /// Có nửa sao hay không
        /// </summary>
        public bool HasHalfStar => AverageRating - FullStars >= 0.25 && AverageRating - FullStars < 0.75;

        /// <summary>
        /// Số sao rỗng
        /// </summary>
        public int EmptyStars => 5 - FullStars - (HasHalfStar ? 1 : 0);
    }
}
