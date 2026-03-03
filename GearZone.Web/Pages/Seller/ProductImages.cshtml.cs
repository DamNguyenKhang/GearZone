using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Seller
{
    public class ProductImagesModel : PageModel
    {
        private readonly IProductImageService _productImageService;

        public ProductImagesModel(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid ProductId { get; set; }

        public List<ProductImageViewModel> Images { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (ProductId == Guid.Empty)
                return RedirectToPage("/Index");

            Images = await _productImageService.GetProductImagesAsync(ProductId);
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        {
            if (ProductId == Guid.Empty)
                return RedirectToPage("/Index");

            if (files == null || !files.Any())
            {
                Errors.Add("Vui lòng chọn ít nhất 1 file.");
                Images = await _productImageService.GetProductImagesAsync(ProductId);
                return Page();
            }

            var result = await _productImageService.UploadProductImagesAsync(ProductId, files);

            if (result.Errors.Any())
                Errors = result.Errors;

            if (result.Success)
                SuccessMessage = $"Đã upload thành công {result.UploadedImages.Count} ảnh.";

            Images = await _productImageService.GetProductImagesAsync(ProductId);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid imageId)
        {
            if (ProductId == Guid.Empty)
                return RedirectToPage("/Index");

            var deleted = await _productImageService.DeleteProductImageAsync(imageId);

            if (deleted)
                SuccessMessage = "Đã xóa ảnh thành công.";
            else
                Errors.Add("Không tìm thấy ảnh để xóa.");

            Images = await _productImageService.GetProductImagesAsync(ProductId);
            return Page();
        }

        public async Task<IActionResult> OnPostSetPrimaryAsync(Guid imageId)
        {
            if (ProductId == Guid.Empty)
                return RedirectToPage("/Index");

            var success = await _productImageService.SetPrimaryImageAsync(ProductId, imageId);

            if (success)
                SuccessMessage = "Đã đặt ảnh làm ảnh chính.";
            else
                Errors.Add("Không thể đặt ảnh chính.");

            Images = await _productImageService.GetProductImagesAsync(ProductId);
            return Page();
        }
    }
}
