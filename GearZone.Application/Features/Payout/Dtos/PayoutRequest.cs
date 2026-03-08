using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Features.Payout.Dtos
{
    public class PayoutRequest
    {
        public long Amount { get; set; }
        public string Description { get; set; } = "";
        public string ToBin { get; set; } = "";
        public string ToAccountNumber { get; set; } = "";
    }
}
