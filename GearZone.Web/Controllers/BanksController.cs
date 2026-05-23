using GearZone.Application.Abstractions.Services;
using GearZone.Web.Common;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers;

[Route("api/banks")]
[ApiController]
public class BanksController : ControllerBase
{
    private readonly IBankCatalogService _bankCatalogService;

    public BanksController(IBankCatalogService bankCatalogService)
    {
        _bankCatalogService = bankCatalogService;
    }

    // GET /api/banks
    [HttpGet]
    public IActionResult GetSupportedBanks()
    {
        return Ok(ApiResponse<object>.Ok(_bankCatalogService.GetSupportedBanks()));
    }
}
