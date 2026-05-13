using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Infranstructure.Persistence.Data;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            return await _paymentRepository.GetPaymentByIdAsync(paymentId);
        }

        public async Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            return await _paymentRepository.GetPaymentByAppointmentIdAsync(appointmentId);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllPaymentsAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId)
        {
            return await _paymentRepository.GetPatientPaymentsAsync(patientId);
        }

        public async Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId)
        {
            return await _paymentRepository.GetDoctorPaymentsAsync(doctorId);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await _paymentRepository.GetPaymentsByStatusAsync(status);
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.UtcNow;

            if (payment.Status == PaymentStatus.Completed && payment.PaidAt == null)
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            _context.Payments.Add(payment);
            await _unitOfWork.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.Status == PaymentStatus.Completed && payment.PaidAt == null)
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            _context.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}