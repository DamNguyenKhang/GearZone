using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Features.Payout.Dtos
{
    public class PayoutResult
    {
        public bool Success { get; set; }
        public string? ReferenceId { get; set; }
        public string? ErrorMessage { get; set; }

        public PayoutResult(bool success, string? referenceId = null, string? errorMessage = null)
        {
            Success = success;
            ReferenceId = referenceId;
            ErrorMessage = errorMessage;
        }
    }
}
