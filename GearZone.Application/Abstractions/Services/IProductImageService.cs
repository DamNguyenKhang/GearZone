using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class UploadResult
    {
        public bool Success { get; set; }
        public List<ProductImageDto> UploadedImages { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public interface IProductImageService
    {
        Task<UploadResult> UploadProductImagesAsync(Guid productId, List<IFormFile> files);
        Task<bool> DeleteProductImageAsync(Guid imageId);
        Task<bool> SetPrimaryImageAsync(Guid productId, Guid imageId);
        Task<List<ProductImageDto>> GetProductImagesAsync(Guid productId);
    }
}
