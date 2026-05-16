using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            return await BuildQuery()
                .Where(p => p.Id == paymentId)
                .Select(MapToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<Payment?> GetPaymentForCheckoutAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment.Patient.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            return await BuildQuery()
                .Where(p => p.AppointmentId == appointmentId)
                .Select(MapToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            return await BuildQuery()
                .OrderByDescending(p => p.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId)
        {
            return await BuildQuery()
                .Where(p => p.Appointment.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId)
        {
            return await BuildQuery()
                .Where(p => p.Appointment.DoctorId == doctorId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await BuildQuery()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        private IQueryable<Payment> BuildQuery()
        {
            return _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor.ApplicationUser)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient.ApplicationUser)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Clinic)
                .AsNoTracking();
        }

        private static Expression<Func<Payment, PaymentDto>> MapToDto()
        {
            return p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaidAt = p.PaidAt,
                TransactionReference = p.TransactionReference,
                FailureReason = p.FailureReason,
                AppointmentId = p.AppointmentId,
                DoctorName = p.Appointment.Doctor.ApplicationUser.FullName,
                PatientName = p.Appointment.Patient.ApplicationUser.FullName,
                ClinicName = p.Appointment.Clinic.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}