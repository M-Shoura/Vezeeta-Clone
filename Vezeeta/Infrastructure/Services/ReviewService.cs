using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
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
            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review == null)
                return false;

            _unitOfWork.Repository<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
