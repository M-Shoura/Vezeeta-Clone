using Application.DTOs.Payments;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId);
        Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId);
        Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<string> CreatePaymobCheckoutUrlAsync(int paymentId, string successUrl, string cancelUrl, string? description = null);
        Task<bool> FinalizePaymobPaymentAsync(int paymentId, string? transactionId = null);
    }
}