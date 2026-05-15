using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Application.Validators;

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

        [RequiredIfEnumValue(nameof(Status), PaymentStatus.Completed)]
        public string? TransactionReference { get; set; }

        [RequiredIfEnumValue(nameof(Status), PaymentStatus.Failed)]
        public string? FailureReason { get; set; }
    }
}