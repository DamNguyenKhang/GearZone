using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderDto
    {
        public long OrderCode { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PayoutStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
