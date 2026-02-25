using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.StoreApplications
{
    public class DetailModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public void OnGet(string id)
        {
            Id = id;
        }
    }
}
