using Azure;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace GearZone.Infrastructure.External
{
    public class PayOSPaymentStrategy : IPaymentStrategy
    {
        private readonly PayOSClient _client;
        private readonly PayOSSettings _settings;
        private readonly ILogger<PayOSPaymentStrategy> _logger;

        public PayOSPaymentStrategy(
            [FromKeyedServices("OrderClient")] PayOSClient client,
            IOptions<PayOSSettings> settings,
            ILogger<PayOSPaymentStrategy> logger)
        {
            _client = client;
            _settings = settings.Value;
            _logger = logger;
        }

        public PaymentMethod Method => PaymentMethod.PayOS;

        public async Task<PaymentResult> ProcessPaymentAsync(Order order)
        {
            try
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                var allItems = order.SubOrders.SelectMany(s => s.Items).ToList();
                if (!allItems.Any())
                    throw new Exception("Order must contain items");

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = order.OrderCode,
                    Amount = (long)order.GrandTotal,
                    Description = $"PAY FOR ORDER {order.OrderCode}",

                    ReturnUrl = _settings.ReturnUrl,
                    CancelUrl = _settings.CancelUrl,

                    BuyerName = order.User?.FullName,
                    BuyerEmail = order.User?.Email,
                    BuyerPhone = order.User?.PhoneNumber,
                    BuyerAddress = order.User?.Address,

                    Items = allItems.Select(i => new PaymentLinkItem
                    {
                        Name = i.ProductNameSnapshot,
                        Quantity = i.Quantity,
                        Price = (int)i.UnitPriceSnapshot
                    }).ToList()
                };

                var response = await _client.PaymentRequests.CreateAsync(paymentRequest);

                return new PaymentResult(
                    success: true,
                    checkoutUrl: response.CheckoutUrl,
                    paymentLinkId: response.PaymentLinkId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS payment creation failed for order {OrderCode}", order?.OrderCode);

                return new PaymentResult(
                    success: false,
                    checkoutUrl: null,
                    errorMessage: ex.Message
                );
            }
        }
    }
}
