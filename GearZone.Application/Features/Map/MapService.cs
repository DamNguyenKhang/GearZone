using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Dtos;
using GearZone.Application.Features.Map.Dtos;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Map;

public class MapService : IMapService
{
    private readonly IGoongService _goongService;

    public MapService(IGoongService goongService)
    {
        _goongService = goongService;
    }

    public async Task<GoongAutocompleteResponse?> GetAutocompleteAsync(string input)
    {
        return await _goongService.GetAutocompleteAsync(input);
    }

    public async Task<AddressDetailDto?> GetAddressDetailAsync(string placeId)
    {
        var placeDetail = await _goongService.GetPlaceDetailAsync(placeId);
        if (placeDetail?.Result == null) return null;

        return MapToAddressDetail(placeDetail.Result);
    }

    public async Task<AddressDetailDto?> GetReverseGeocodeAsync(double lat, double lng)
    {
        var response = await _goongService.GetReverseGeocodeAsync(lat, lng);
        if (response?.Results == null || !response.Results.Any()) return null;

        return MapToAddressDetail(response.Results.First());
    }

    private AddressDetailDto MapToAddressDetail(GoongPlaceResult result)
    {
        var components = result.AddressComponents;
        var findComp = (string[] types) => components.FirstOrDefault(c => types.Any(t => c.Types.Contains(t)))?.LongName;

        var detail = new AddressDetailDto
        {
            FullAddress = result.FormattedAddress,
            Lat = result.Geometry.Location.Lat,
            Lng = result.Geometry.Location.Lng,
            Province = findComp(new[] { "administrative_area_level_1" }),
            District = findComp(new[] { "administrative_area_level_2" }),
            Ward = findComp(new[] { "sublocality_level_1", "administrative_area_level_3", "sublocality" })
        };

        // Fallback: If components are missing, attempt to parse from formatted address
        if (string.IsNullOrEmpty(detail.Province) || string.IsNullOrEmpty(detail.District) || string.IsNullOrEmpty(detail.Ward))
        {
            var parts = result.FormattedAddress.Split(',')
                .Select(s => s.Trim())
                .Where(s => !s.Equals("Việt Nam", System.StringComparison.OrdinalIgnoreCase))
                .Reverse()
                .ToList();

            if (string.IsNullOrEmpty(detail.Province) && parts.Count > 0) detail.Province = parts[0];
            if (string.IsNullOrEmpty(detail.District) && parts.Count > 1) detail.District = parts[1];
            if (string.IsNullOrEmpty(detail.Ward) && parts.Count > 2) detail.Ward = parts[2];
        }

        return detail;
    }
}
