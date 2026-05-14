using Application.DTOs.Reviews;

namespace Application.Interfaces.Repositories
{
    public interface IReviewRepository
    {
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<IEnumerable<ReviewDto>> GetDoctorReviewsAsync(string doctorId);
        Task<IEnumerable<ReviewDto>> GetPatientReviewsAsync(string patientId);
        Task<IEnumerable<ReviewDto>> GetReviewsByAppointmentAsync(int appointmentId);
        Task<double> GetDoctorAverageRatingAsync(string doctorId);
    }
}
