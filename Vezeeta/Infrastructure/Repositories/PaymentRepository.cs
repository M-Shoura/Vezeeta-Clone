using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await BuildQuery()
                .Where(p => p.Id == paymentId)
                .FirstOrDefaultAsync();

            return ToDto(payment);
        }

        public async Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            var payment = await BuildQuery()
                .Where(p => p.AppointmentId == appointmentId)
                .FirstOrDefaultAsync();

            return ToDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await BuildQuery()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(ToDto).OfType<PaymentDto>();
        }

        public async Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId)
        {
            var payments = await BuildQuery()
                .Where(p => p.Appointment.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(ToDto).OfType<PaymentDto>();
        }

        public async Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId)
        {
            var payments = await BuildQuery()
                .Where(p => p.Appointment.DoctorId == doctorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(ToDto).OfType<PaymentDto>();
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            var payments = await BuildQuery()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(ToDto).OfType<PaymentDto>();
        }

        private IQueryable<Payment> BuildQuery()
        {
            return _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.ApplicationUser)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.ApplicationUser)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Clinic)
                .AsNoTracking();
        }

        private static PaymentDto? ToDto(Payment? payment)
        {
            if (payment == null || payment.Appointment == null)
                return null;

            return new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaidAt = payment.PaidAt,
                TransactionReference = payment.TransactionReference,
                FailureReason = payment.FailureReason,
                AppointmentId = payment.AppointmentId,
                AppointmentDate = payment.Appointment.AppointmentDate,
                StartTime = payment.Appointment.StartTime,
                EndTime = payment.Appointment.EndTime,
                DoctorId = payment.Appointment.DoctorId,
                DoctorName = payment.Appointment.Doctor.ApplicationUser.FullName,
                PatientId = payment.Appointment.PatientId,
                PatientName = payment.Appointment.Patient.ApplicationUser.FullName,
                ClinicName = payment.Appointment.Clinic.Name,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}