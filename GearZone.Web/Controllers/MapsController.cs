using GearZone.Application.Features.Map;
using GearZone.Web.Common;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers;

[ApiController]
[Route("api/maps")]
public class MapsController : ControllerBase
{
    private readonly IMapService _mapService;

    public MapsController(IMapService mapService)
    {
        _mapService = mapService;
    }

    // GET /api/maps/autocomplete?input=
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string input)
    {
        var result = await _mapService.GetAutocompleteAsync(input);
        if (result == null) return BadRequest(ApiResponse.Fail("No results."));
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/maps/place-detail?placeId=
    [HttpGet("place-detail")]
    public async Task<IActionResult> PlaceDetail([FromQuery] string placeId)
    {
        var result = await _mapService.GetAddressDetailAsync(placeId);
        if (result == null) return BadRequest(ApiResponse.Fail("Place not found."));
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/maps/reverse-geocode?lat=&lng=
    [HttpGet("reverse-geocode")]
    public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lng)
    {
        var result = await _mapService.GetReverseGeocodeAsync(lat, lng);
        if (result == null) return BadRequest(ApiResponse.Fail("Geocode failed."));
        return Ok(ApiResponse<object>.Ok(result));
    }
}
