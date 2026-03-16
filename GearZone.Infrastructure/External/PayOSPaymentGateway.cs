using GearZone.Application.Abstractions.External;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayOS;

namespace GearZone.Infrastructure.External
{
    public class PayOSPaymentGateway : IPaymentGateway
    {
        private readonly PayOSClient _client;
        private readonly ILogger<PayOSPaymentGateway> _logger;

        public PayOSPaymentGateway(
            [FromKeyedServices("OrderClient")] PayOSClient client,
            ILogger<PayOSPaymentGateway> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<PaymentGatewayResult> GetPaymentStatusAsync(long orderCode)
        {
            try
            {
                var paymentInfo = await _client.PaymentRequests.GetAsync(orderCode);

                if (paymentInfo == null)
                    return PaymentGatewayResult.Error("Could not retrieve payment info from PayOS.");

                var statusStr = paymentInfo.Status.ToString().ToUpperInvariant();

                _logger.LogInformation(
                    "PayOS payment status for order {OrderCode}: {Status}",
                    orderCode, statusStr);

                // PayOS PaymentLinkStatus enum: compare using string representation
                if (statusStr == "PAID")
                {
                    return PaymentGatewayResult.Paid(paymentInfo.Id.ToString());
                }

                return PaymentGatewayResult.NotPaid(statusStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying PayOS for order {OrderCode}", orderCode);
                return PaymentGatewayResult.Error(ex.Message);
            }
        }
    }
}
