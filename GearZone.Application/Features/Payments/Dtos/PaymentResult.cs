namespace GearZone.Application.Features.Payment.Dtos
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? ErrorMessage { get; set; }

        public PaymentResult(bool success, string? checkoutUrl, string? paymentLinkId = null, string? errorMessage = null)
        {
            Success = success;
            CheckoutUrl = checkoutUrl;
            PaymentLinkId = paymentLinkId;
            ErrorMessage = errorMessage;
        }
    }
}
