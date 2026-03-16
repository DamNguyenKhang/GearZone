using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Web.Pages.Admin.Products
{
    public class DetailModel : PageModel
    {
        private readonly IAdminProductService _productService;

        public DetailModel(IAdminProductService productService)
        {
            _productService = productService;
        }

        public AdminProductDetailDto Product { get; set; } = new AdminProductDetailDto();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToPage("./Index");
            }

            var product = await _productService.GetProductDetailAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Active);
            if (success)
                TempData["SuccessMessage"] = "Product approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Rejected);
            if (success)
                TempData["SuccessMessage"] = "Product rejected.";
            else
                TempData["ErrorMessage"] = "Failed to reject product.";

            return RedirectToPage(new { id });
        }
    }
}