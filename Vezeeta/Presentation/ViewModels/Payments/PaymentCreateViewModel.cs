using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Payments
{
    public class PaymentCreateViewModel
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        public string? Description { get; set; }
    }
}