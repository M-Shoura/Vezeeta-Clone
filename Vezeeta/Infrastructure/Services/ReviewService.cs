using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infranstructure.Persistence.Data;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            return await _reviewRepository.GetReviewByIdAsync(reviewId);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllReviewsAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetDoctorReviewsAsync(string doctorId)
        {
            return await _reviewRepository.GetDoctorReviewsAsync(doctorId);
        }

        public async Task<IEnumerable<ReviewDto>> GetPatientReviewsAsync(string patientId)
        {
            return await _reviewRepository.GetPatientReviewsAsync(patientId);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByAppointmentAsync(int appointmentId)
        {
            return await _reviewRepository.GetReviewsByAppointmentAsync(appointmentId);
        }

        public async Task<double> GetDoctorAverageRatingAsync(string doctorId)
        {
            return await _reviewRepository.GetDoctorAverageRatingAsync(doctorId);
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _unitOfWork.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
