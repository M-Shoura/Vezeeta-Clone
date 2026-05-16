using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
                .Select(MapToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.Doctor.ApplicationUser)
                .Include(r => r.Patient.ApplicationUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetDoctorReviewsAsync(string doctorId)
        {
            return await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Doctor.ApplicationUser)
                .Include(r => r.Patient.ApplicationUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetPatientReviewsAsync(string patientId)
        {
            return await _context.Reviews
                .Where(r => r.PatientId == patientId)
                .Include(r => r.Doctor.ApplicationUser)
                .Include(r => r.Patient.ApplicationUser)
                .OrderByDescending(r => r.CreatedAt)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByAppointmentAsync(int appointmentId)
        {
            return await _context.Reviews
                .Where(r => r.AppointmentId == appointmentId)
                .Include(r => r.Doctor.ApplicationUser)
                .Include(r => r.Patient.ApplicationUser)
                .Select(MapToDto())
                .ToListAsync();
        }

        public async Task<double> GetDoctorAverageRatingAsync(string doctorId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.DoctorId == doctorId)
                .Select(r => r.Rating)
                .ToListAsync();

            return reviews.Count == 0 ? 0 : reviews.Average();
        }

        private static Expression<Func<Review, ReviewDto>> MapToDto()
        {
            return r => new ReviewDto
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
            };
        }
    }
}
