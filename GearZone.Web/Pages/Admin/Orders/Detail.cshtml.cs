using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace GearZone.Web.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class DetailModel : PageModel
    {
        public void OnGet(Guid? id)
        {
            // Empty placeholder for prototype implementation
            // Real implementation will fetch the Order with OrderItem, Store and Payment by Id
        }
    }
}
