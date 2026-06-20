using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/stores")]
[ApiController]
public class StoreManagementController : BaseApiController
{
    private readonly IAdminStoreService _storeService;

    public StoreManagementController(IAdminStoreService storeService)
    {
        _storeService = storeService;
    }

    // GET /api/admin/stores
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var stores = await _storeService.GetAllStoresAsync();
        return OkResponse(stores);
    }

    // GET /api/admin/stores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var store = await _storeService.GetStoreApplicationByIdAsync(id);
        if (store == null) return FailResponse("Store not found.", 404);
        return OkResponse(store);
    }
}
