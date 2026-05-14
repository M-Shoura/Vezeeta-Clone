using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Where(r => r.Id == reviewId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AppointmentId = r.AppointmentId,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    DoctorName = r.Doctor.ApplicationUser.FullName,
                    PatientName = r.Patient.ApplicationUser.FullName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AppointmentId = r.AppointmentId,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    DoctorName = r.Doctor.ApplicationUser.FullName,
                    PatientName = r.Patient.ApplicationUser.FullName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetDoctorReviewsAsync(string doctorId)
        {
            return await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AppointmentId = r.AppointmentId,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    DoctorName = r.Doctor.ApplicationUser.FullName,
                    PatientName = r.Patient.ApplicationUser.FullName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetPatientReviewsAsync(string patientId)
        {
            return await _context.Reviews
                .Where(r => r.PatientId == patientId)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AppointmentId = r.AppointmentId,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    DoctorName = r.Doctor.ApplicationUser.FullName,
                    PatientName = r.Patient.ApplicationUser.FullName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByAppointmentAsync(int appointmentId)
        {
            return await _context.Reviews
                .Where(r => r.AppointmentId == appointmentId)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.ApplicationUser)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.ApplicationUser)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AppointmentId = r.AppointmentId,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    DoctorName = r.Doctor.ApplicationUser.FullName,
                    PatientName = r.Patient.ApplicationUser.FullName
                })
                .ToListAsync();
        }

        public async Task<double> GetDoctorAverageRatingAsync(string doctorId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            return reviews.Average();
        }
    }
}
