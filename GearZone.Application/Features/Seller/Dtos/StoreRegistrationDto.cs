using System;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class StoreRegistrationDto
    {
        public string OwnerUserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
    }
}
