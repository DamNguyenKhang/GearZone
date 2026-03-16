using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly IAdminVoucherService _voucherService;
        private readonly IAdminCategoryService _categoryService;

        public CreateModel(IAdminVoucherService voucherService, IAdminCategoryService categoryService)
        {
            _voucherService = voucherService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public CreateVoucherDto Input { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllCategoriesListAsync();
            
            // Set default values
            Input.StartAt = DateTime.Now;
            Input.EndAt = DateTime.Now.AddDays(30);
            Input.UsageLimit = 1000;
            Input.DiscountType = "Percent";
            Input.DiscountValue = 10;
            Input.MinOrderAmount = 50000;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Custom Validation
            if (Input.DiscountType == "Percent" && Input.DiscountValue >= 100)
            {
                ModelState.AddModelError("Input.DiscountValue", "Percentage must be under 100%");
            }
            if (Input.DiscountType == "Fixed" && Input.MinOrderAmount <= Input.DiscountValue)
            {
                ModelState.AddModelError("Input.MinOrderAmount", "Minimum spend must be greater than discount amount");
            }

            if (Input.EndAt <= Input.StartAt)
            {
                ModelState.AddModelError("Input.EndAt", "Expiration date must be later than launch date");
            }

            if (Input.DiscountType == "Fixed")
            {
                Input.MaxDiscount = null;
            }

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["ErrorMessage"] = firstError ?? "Please check the form for errors.";
                Categories = await _categoryService.GetAllCategoriesListAsync();
                return Page();
            }

            var result = await _voucherService.CreateVoucherAsync(Input);
            if (result)
            {
                TempData["SuccessMessage"] = "Voucher created successfully!";
                return RedirectToPage("./Index");
            }

            TempData["ErrorMessage"] = "Failed to create voucher. Please check the information and try again.";
            ModelState.AddModelError("", "Failed to create voucher. Please check the information and try again.");
            Categories = await _categoryService.GetAllCategoriesListAsync();
            return Page();
        }
    }
}
