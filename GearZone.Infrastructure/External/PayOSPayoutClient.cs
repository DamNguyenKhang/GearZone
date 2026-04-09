using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payout.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V1.Payouts.Batch;
using PayOS.Models.V1.PayoutsAccount;

namespace GearZone.Infrastructure.External
{
    public class PayOSPayoutClient : IPayoutClient
    {
        private readonly PayOSClient _client;
        private readonly ILogger<PayOSPayoutClient> _logger;

        public PayOSPayoutClient(
            [FromKeyedServices("TransferClient")] PayOSClient client,
            ILogger<PayOSPayoutClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<PayoutResult> CreatePayoutAsync(PayoutRequestDto payout)
        {
            var request = new PayoutRequest
            {
                ReferenceId = Guid.NewGuid().ToString(),
                Amount = payout.Amount,
                Description = payout.Description,
                ToBin = payout.ToBin,
                ToAccountNumber = payout.ToAccountNumber
            };

            try
            {
                var response = await _client.Payouts.CreateAsync(request);

                return new PayoutResult(
                    isSuccess: true,
                    referenceId: response.ReferenceId
                );
            }
            catch (Exception ex)
            {
                return new PayoutResult(
                    isSuccess: false,
                    errorMessage: ex.Message
                );
            }
        }

        public async Task<PayoutResult> CreateBatchPayoutAsync(List<PayoutRequestDto> payouts)
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
                    isSuccess: true,
                    referenceId: response.ReferenceId
                );
            }
            catch (Exception ex)
            {
                return new PayoutResult(
                    isSuccess: false,
                    errorMessage: ex.Message
                );
            }
        }

        public async Task<PayoutAccountInfoDto> GetAccountBalance()
        {
            try
            {
                var payoutAccount = await _client.PayoutsAccount.GetBalanceAsync();
                
                if (payoutAccount == null)
                {
                    _logger.LogWarning("PayOS GetBalanceAsync returned null.");
                    return null!;
                }

                return new PayoutAccountInfoDto
                {
                    AccountName = payoutAccount.AccountName ?? string.Empty,
                    AccountNumber = payoutAccount.AccountNumber ?? string.Empty,
                    Balance = payoutAccount.Balance.ToString(),
                    Currency = payoutAccount.Currency ?? string.Empty,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS account balance");
                return null!;
            }
        }
    }
}
