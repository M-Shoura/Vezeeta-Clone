using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Application.DTOs.Reviews;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.ViewModels.Reviews;
using System.Security.Claims;

namespace Presentation.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewController(IReviewService reviewService, IUnitOfWork unitOfWork)
        {
            _reviewService = reviewService;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return View(reviews);
        }

        [Authorize(Roles = "Patient,Admin,Doctor")]
        public async Task<IActionResult> PatientReviews(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Forbid();

            var reviews = await _reviewService.GetPatientReviewsAsync(id);
            return View(reviews);
        }

        [Authorize(Roles = "Patient,Admin,Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            if (!CanAccessReview(review))
                return Forbid();

            SetBackNavigation();
            return View(review);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create(int appointmentId = 0)
        {
            if (appointmentId > 0)
            {
                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound();

                if (appointment.PatientId != GetCurrentUserId())
                    return Forbid();

                if (!IsAppointmentCompleted(appointment))
                    return Forbid();

                var existingReview = await _unitOfWork.Repository<Review>().FindAsync(r => r.AppointmentId == appointmentId);
                if (existingReview != null)
                    return RedirectToAction(nameof(Details), new { id = existingReview.Id });
            }

            return View(new ReviewCreateViewModel { AppointmentId = appointmentId });
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Forbid();

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(vm.AppointmentId);
            if (appointment == null || appointment.PatientId != currentUserId || !IsAppointmentCompleted(appointment))
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Invalid appointment.");
                return View(vm);
            }

            var existingReview = await _unitOfWork.Repository<Review>().FindAsync(r => r.AppointmentId == vm.AppointmentId);
            if (existingReview != null)
                return RedirectToAction(nameof(Details), new { id = existingReview.Id });

            var review = new Review
            {
                Rating = vm.Rating,
                Comment = vm.Comment,
                AppointmentId = vm.AppointmentId,
                DoctorId = appointment.DoctorId!,
                PatientId = appointment.PatientId!
            };

            try
            {
                var added = await _reviewService.AddReviewAsync(review);
                TempData["Success"] = "Review added successfully";
                return RedirectToAction(nameof(Details), new { id = added.Id });
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save this review. A review may already exist for the appointment.");
                return View(vm);
            }
        }

        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

            if (!CanDeleteReview(dto))
                return Forbid();

            return View(dto);
        }

        [Authorize(Roles = "Patient,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null || !CanDeleteReview(dto))
                return Forbid();

            await _reviewService.DeleteReviewAsync(id);
            TempData["Success"] = "Review deleted successfully";

            return IsPatientUser() 
                ? RedirectToAction(nameof(PatientController.Dashboard), "Patient") 
                : RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null || !CanAccessReview(dto))
                return Forbid();

            var vm = new ReviewCreateViewModel
            {
                AppointmentId = dto.AppointmentId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            ViewData["ReviewId"] = dto.Id;
            return View(vm);
        }

        [Authorize(Roles = "Patient,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReviewId"] = id;
                return View(vm);
            }

            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null || !CanAccessReview(dto))
                return Forbid();

            var reviewEntity = await _unitOfWork.Repository<Review>().GetByIdAsync(id);
            if (reviewEntity == null)
                return NotFound();

            reviewEntity.Rating = vm.Rating;
            reviewEntity.Comment = vm.Comment;

            await _reviewService.UpdateReviewAsync(reviewEntity);
            TempData["Success"] = "Review updated successfully";

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool CanAccessReview(ReviewDto review) => IsAdminUser() || (IsPatientUser() && review.PatientId == GetCurrentUserId()) || (IsDoctorUser() && review.DoctorId == GetCurrentUserId());

        private bool CanDeleteReview(ReviewDto review) => IsAdminUser() || (IsPatientUser() && review.PatientId == GetCurrentUserId());

        private bool IsAppointmentCompleted(Appointment appointment) => appointment.AppointmentDate.Date.Add(appointment.EndTime) <= DateTime.Now;

        private void SetBackNavigation()
        {
            if (!IsPatientUser())
                return;

            ViewData["BackController"] = "Patient";
            ViewData["BackAction"] = nameof(PatientController.Dashboard);
        }

        private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool IsAdminUser() => User.IsInRole("Admin");
        private bool IsDoctorUser() => User.IsInRole("Doctor");
        private bool IsPatientUser() => User.IsInRole("Patient");
    }
}
