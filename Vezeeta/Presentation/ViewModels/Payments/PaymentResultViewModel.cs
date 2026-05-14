namespace Presentation.ViewModels.Payments
{
    public class PaymentResultViewModel
    {
        public int PaymentId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}