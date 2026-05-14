using Domain.Enums;

namespace Application.DTOs.Payments
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionReference { get; set; }
        public string? FailureReason { get; set; }
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}