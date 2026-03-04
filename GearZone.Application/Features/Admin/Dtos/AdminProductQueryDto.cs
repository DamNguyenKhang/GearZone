using System;
using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public string? SearchType { get; set; } // Name, SKU, Store
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public Guid? StoreId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool OutOfStock { get; set; }
    }
}
