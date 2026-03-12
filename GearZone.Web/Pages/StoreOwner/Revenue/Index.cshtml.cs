using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Revenue
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
