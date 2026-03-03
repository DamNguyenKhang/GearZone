using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner
{
    [Authorize(Roles = "Store Owner")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
