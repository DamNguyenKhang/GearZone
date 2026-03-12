using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Checkout.Dtos
{
    public class CheckoutRequestDto
    {
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
        public ShippingInfoDto ShippingInfo { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public bool SaveAddress { get; set; }
    }

    public class ShippingInfoDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string StreetAddress { get; set; } = null!;
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Ward { get; set; } = null!;
    }

    public class CheckoutResponseDto
    {
        public bool Success { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
