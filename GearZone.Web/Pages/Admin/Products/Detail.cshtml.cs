using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;

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
            await _productService.ApproveProductAsync(id);
            TempData["SuccessMessage"] = "Product has been approved and is now active.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason = "")
        {
            await _productService.RejectProductAsync(id, reason);
            TempData["SuccessMessage"] = "Product request has been rejected.";
            return RedirectToPage(new { id });
        }
    }
}
