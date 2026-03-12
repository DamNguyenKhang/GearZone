using GearZone.Application.Features.Checkout.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDto> ProcessCheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default);
    }
}
