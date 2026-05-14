using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Presentation.ViewModels.Payments
{
    public class PaymentEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        public string? TransactionReference { get; set; }

        public string? FailureReason { get; set; }
    }
}