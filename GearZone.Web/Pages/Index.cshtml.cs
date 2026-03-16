using System.Security.Claims;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public IndexModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public HomePageDto HomePage { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HomePage = await _catalogService.GetHomePageAsync(currentUserId);
        }
    }
}
