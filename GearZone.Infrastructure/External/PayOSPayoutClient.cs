using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payout.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V1.Payouts.Batch;

namespace GearZone.Infrastructure.External
{
    public class PayOSPayoutClient : IPayoutClient
    {
        private readonly PayOSClient _client;
        private readonly ILogger<PayOSPayoutClient> _logger;

        public PayOSPayoutClient(
            [FromKeyedServices("OrderClient")] PayOSClient client,
            ILogger<PayOSPayoutClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<PayoutResult> CreateBatchPayoutAsync(List<PayoutRequest> payouts)
        {
            var request = new PayoutBatchRequest
            {
                ReferenceId = Guid.NewGuid().ToString(),
                Payouts = payouts.Select(p => new PayoutBatchItem
                {
                    ReferenceId = Guid.NewGuid().ToString(),
                    Amount = p.Amount,
                    Description = p.Description,
                    ToBin = p.ToBin,
                    ToAccountNumber = p.ToAccountNumber
                }).ToList()
            };

            try
            {
                var response = await _client.Payouts.Batch.CreateAsync(request);

                return new PayoutResult(
                    success: true,
                    referenceId: response.ReferenceId
                );
            }
            catch (Exception ex)
            {
                return new PayoutResult(
                    success: false,
                    errorMessage: ex.Message
                );
            }
        }
    }
}
