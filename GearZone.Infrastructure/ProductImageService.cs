using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Abstractions.External;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure
{
    public class ProductImageService : IProductImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        private const int MaxImagesPerProduct = 10;
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

        public ProductImageService(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<UploadResult> UploadProductImagesAsync(Guid productId, List<IFormFile> files)
        {
            var result = new UploadResult();

            // Validate product exists
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            if (product == null)
            {
                result.Errors.Add("Sản phẩm không tồn tại.");
                return result;
            }

            // Check current image count
            var currentCount = await _context.ProductImages
                .CountAsync(pi => pi.ProductId == productId);

            if (currentCount + files.Count > MaxImagesPerProduct)
            {
                result.Errors.Add($"Tối đa {MaxImagesPerProduct} ảnh/sản phẩm. Hiện có {currentCount}, bạn chỉ có thể thêm {MaxImagesPerProduct - currentCount} ảnh nữa.");
                return result;
            }

            // Validate each file
            var validFiles = new List<IFormFile>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName);
                if (!AllowedExtensions.Contains(ext))
                {
                    result.Errors.Add($"File '{file.FileName}' không hợp lệ. Chỉ chấp nhận: jpg, png, webp.");
                    continue;
                }
                if (!AllowedMimeTypes.Contains(file.ContentType))
                {
                    result.Errors.Add($"File '{file.FileName}' có định dạng nội dung không hợp lệ.");
                    continue;
                }
                if (file.Length > MaxFileSizeBytes)
                {
                    result.Errors.Add($"File '{file.FileName}' vượt quá 5MB.");
                    continue;
                }
                if (file.Length == 0)
                {
                    result.Errors.Add($"File '{file.FileName}' trống.");
                    continue;
                }
                validFiles.Add(file);
            }

            if (!validFiles.Any())
            {
                if (!result.Errors.Any())
                    result.Errors.Add("Không có file hợp lệ để upload.");
                return result;
            }

            // Upload to Cloudinary
            var uploadRequests = validFiles.Select(f =>
                new FileUploadRequest(f.FileName, f.OpenReadStream(), f.Length)).ToList();
            try
            {
                var imageUrls = await _fileStorage.UploadAsync(uploadRequests);

            // Determine if product already has a primary image
            var hasPrimary = await _context.ProductImages
                .AnyAsync(pi => pi.ProductId == productId && pi.IsPrimary);

            var maxSortOrder = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .Select(pi => (int?)pi.SortOrder)
                .MaxAsync() ?? -1;

            // Save to database
            var newImages = new List<ProductImage>();
            for (int i = 0; i < imageUrls.Count; i++)
            {
                var image = new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ImageUrl = imageUrls[i],
                    IsPrimary = !hasPrimary && i == 0,
                    SortOrder = maxSortOrder + 1 + i
                };
                newImages.Add(image);
            }

            await _context.ProductImages.AddRangeAsync(newImages);
            await _context.SaveChangesAsync();

            result.Success = true;
            result.UploadedImages = newImages.Select(MapToViewModel).ToList();
            return result;
            }
            finally
            {
                // Dispose streams to prevent memory leaks
                foreach (var req in uploadRequests)
                {
                    await req.Stream.DisposeAsync();
                }
            }
        }

        public async Task<bool> DeleteProductImageAsync(Guid imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) return false;

            // Delete from Cloudinary
            await _fileStorage.DeleteAsync(image.ImageUrl);

            var wasPrimary = image.IsPrimary;
            var productId = image.ProductId;

            // Delete from database
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // If deleted image was primary, set next image as primary
            if (wasPrimary)
            {
                var nextImage = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .OrderBy(pi => pi.SortOrder)
                    .FirstOrDefaultAsync();

                if (nextImage != null)
                {
                    nextImage.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> SetPrimaryImageAsync(Guid productId, Guid imageId)
        {
            var images = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            if (!images.Any(i => i.Id == imageId)) return false;

            foreach (var img in images)
            {
                img.IsPrimary = img.Id == imageId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductImageViewModel>> GetProductImagesAsync(Guid productId)
        {
            var images = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.SortOrder)
                .AsNoTracking()
                .ToListAsync();

            return images.Select(MapToViewModel).ToList();
        }

        private static ProductImageViewModel MapToViewModel(ProductImage image)
        {
            return new ProductImageViewModel
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                SortOrder = image.SortOrder
            };
        }
    }
}
