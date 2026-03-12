using System.ComponentModel.DataAnnotations;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class TopupWalletDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Bank transfer reference is required")]
        [MaxLength(100)]
        public string BankTransferReference { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
