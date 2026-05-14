using Application.DTOs.Payments;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<Payment?> GetPaymentForCheckoutAsync(int paymentId);
        Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId);
        Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId);
        Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status);
    }
}