using Application.DTOs.Reviews;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetDoctorReviewsAsync(string doctorId);
        Task<IEnumerable<ReviewDto>> GetPatientReviewsAsync(string patientId);
        Task<IEnumerable<ReviewDto>> GetReviewsByAppointmentAsync(int appointmentId);
        Task<double> GetDoctorAverageRatingAsync(string doctorId);
        Task<Review> AddReviewAsync(Review review);
        Task<Review> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
