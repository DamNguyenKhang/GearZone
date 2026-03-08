using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.StoreManagement
{
    public class DetailModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;
        private readonly IAdminProductService _adminProductService;

        public DetailModel(IAdminStoreService adminStoreService, IAdminProductService adminProductService)
        {
            _adminStoreService = adminStoreService;
            _adminProductService = adminProductService;
        }

        public StoreApplicationDto? StoreApplication { get; set; }
        public PagedResult<AdminProductDto> Products { get; set; } = new(new List<AdminProductDto>(), 0, 1, 5);

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            StoreApplication = await _adminStoreService.GetStoreApplicationByIdAsync(id);

            if (StoreApplication == null)
                return NotFound();

            var query = new AdminProductQueryDto 
            { 
                StoreId = id, 
                PageNumber = 1, 
                PageSize = 5,
                SortBy = "createdAt",
                SortDirection = "desc"
            };
            Products = await _adminProductService.GetProductsAsync(query);

            return Page();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(Guid id, StoreStatus status, string reason = "")
        {
            var success = await _adminStoreService.UpdateStoreStatusAsync(id, status, reason);

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to change store status.";
                return RedirectToPage(new { id });
            }

            TempData["SuccessMessage"] = $"Store status has been changed to {status}.";
            return RedirectToPage(new { id });
        }
    }
}
