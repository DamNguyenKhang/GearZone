using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Web.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IAdminOrderService _orderService;

        public IndexModel(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminOrderQueryDto Query { get; set; } = new AdminOrderQueryDto();

        public PagedResult<AdminOrderDto> Orders { get; set; } = new PagedResult<AdminOrderDto>();
        public AdminOrderStatsDto Stats { get; set; } = new AdminOrderStatsDto();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Query.DateRange) && Query.DateRange.ToLower() != "custom")
            {
                var today = System.DateTime.UtcNow.Date;
                switch (Query.DateRange.ToLower())
                {
                    case "today":
                        Query.StartDate = today;
                        Query.EndDate = today;
                        break;
                    case "week":
                        Query.StartDate = today.AddDays(-7);
                        Query.EndDate = today;
                        break;
                    case "month":
                        Query.StartDate = today.AddDays(-30);
                        Query.EndDate = today;
                        break;
                    case "year":
                        Query.StartDate = today.AddDays(-365);
                        Query.EndDate = today;
                        break;
                }
            }

            Stats = await _orderService.GetOrderStatsAsync();
            Orders = await _orderService.GetOrdersAsync(Query);
        }
    }
}
