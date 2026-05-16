namespace Presentation.ViewModels.Payments
{
    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
    }
}