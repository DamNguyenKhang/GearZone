using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        private readonly ISellerVoucherService _sellerVoucherService;
        private readonly IAdminCategoryService _categoryService;

        public CreateModel(ISellerVoucherService sellerVoucherService, IAdminCategoryService categoryService)
        {
            _sellerVoucherService = sellerVoucherService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public SellerCreateVoucherDto Input { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllCategoriesListAsync();
            Input.StartAt = DateTime.Now;
            Input.EndAt = DateTime.Now.AddDays(30);
            Input.DiscountType = "Percent";
            Input.DiscountValue = 10;
            Input.MinOrderAmount = 50000;
            Input.UsageLimit = 100;
            Input.IsVisible = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _categoryService.GetAllCategoriesListAsync();

            if (Input.DiscountType == "Fixed")
            {
                Input.MaxDiscount = null;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var (success, error) = await _sellerVoucherService.CreateVoucherAsync(ownerUserId, Input);
            if (success)
            {
                TempData["SuccessMessage"] = "Voucher created successfully.";
                return RedirectToPage("./Index");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError(string.Empty, error);
                TempData["ErrorMessage"] = error;
            }

            return Page();
        }
    }
}
