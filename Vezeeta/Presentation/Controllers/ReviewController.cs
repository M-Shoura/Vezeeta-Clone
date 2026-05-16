using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
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

        // GET: /Review
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return View(reviews);
        }

        // GET: /Review/DoctorReviews/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DoctorReviews(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var reviews = await _reviewService.GetDoctorReviewsAsync(id);

            return View(reviews);
        }

        // GET: /Review/PatientReviews/id
        [Authorize(Roles = "Patient,Admin,Doctor")]
        public async Task<IActionResult> PatientReviews(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var currentUserId = GetCurrentUserId();
            if (IsPatientUser() && id != currentUserId)
                return Forbid();

            var reviews = await _reviewService.GetPatientReviewsAsync(id);

            return View(reviews);
        }

        // GET: /Review/Details/id
        [Authorize(Roles = "Patient,Admin,Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (IsPatientUser() && review.PatientId != currentUserId)
                return Forbid();

            if (IsDoctorUser() && review.DoctorId != currentUserId && !IsAdminUser())
                return Forbid();

            if (IsPatientUser())
            {
                ViewData["BackController"] = "Patient";
                ViewData["BackAction"] = nameof(PatientController.Dashboard);
            }
            else
            {
                ViewData["BackController"] = "Review";
                ViewData["BackAction"] = nameof(Index);
            }

            return View(review);
        }

        // GET: /Review/Create
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create(int appointmentId = 0)
        {
            if (appointmentId > 0)
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Challenge();

                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound();

                if (appointment.PatientId != currentUserId)
                    return Forbid();

                if (appointment.Status != AppointmentStatus.Completed)
                    return Forbid();

                var existingReview = await _unitOfWork.Repository<Review>()
                    .FindAsync(r => r.AppointmentId == appointmentId);
                if (existingReview != null)
                    return RedirectToAction(nameof(Details), new { id = existingReview.Id });
            }

            var vm = new ReviewCreateViewModel
            {
                AppointmentId = appointmentId,
            };

            return View(vm);
        }

        // POST: /Review/Create
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
            if (appointment == null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Appointment not found.");
                return View(vm);
            }

            if (appointment.PatientId != currentUserId)
                return Forbid();

            if (appointment.Status != AppointmentStatus.Completed)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "You can only review completed appointments.");
                return View(vm);
            }

            var existingReview = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.AppointmentId == vm.AppointmentId);
            if (existingReview != null)
            {
                return RedirectToAction(nameof(Details), new { id = existingReview.Id });
            }

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
                await _reviewService.AddReviewAsync(review);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save this review. A review may already exist for the appointment.");
                return View(vm);
            }

            TempData["Success"] = "Review added successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Review/Edit/id
        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (!IsAdminUser() && dto.PatientId != currentUserId)
                return Forbid();

            var vm = new ReviewEditViewModel
            {
                Id = dto.Id,
                Rating = dto.Rating,
                Comment = dto.Comment,
                AppointmentId = dto.AppointmentId,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            return View(vm);
        }

        // POST: /Review/Edit/id
        [Authorize(Roles = "Patient,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewEditViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var existingReview = await _reviewService.GetReviewByIdAsync(id);
            if (existingReview == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (!IsAdminUser() && existingReview.PatientId != currentUserId)
                return Forbid();

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(existingReview.AppointmentId);
            if (appointment == null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Appointment not found.");
                return View(vm);
            }

            var conflictingReview = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.AppointmentId == existingReview.AppointmentId && r.Id != vm.Id);
            if (conflictingReview != null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "A review already exists for this appointment.");
                return View(vm);
            }

            var review = new Review
            {
                Id = vm.Id,
                Rating = vm.Rating,
                Comment = vm.Comment,
                AppointmentId = existingReview.AppointmentId,
                DoctorId = existingReview.DoctorId,
                PatientId = existingReview.PatientId
            };

            await _reviewService.UpdateReviewAsync(review);

            TempData["Success"] = "Review updated successfully";

            return RedirectToAction(nameof(Details), new { id = review.Id });
        }

        // GET: /Review/Delete/id
        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (!IsAdminUser() && dto.PatientId != currentUserId)
                return Forbid();

            return View(dto);
        }

        // POST: /Review/Delete/id
        [Authorize(Roles = "Patient,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (!IsAdminUser() && dto.PatientId != currentUserId)
                return Forbid();

            await _reviewService.DeleteReviewAsync(id);

            TempData["Success"] = "Review deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool IsAdminUser()
        {
            return User.IsInRole("Admin");
        }

        private bool IsDoctorUser()
        {
            return User.IsInRole("Doctor");
        }

        private bool IsPatientUser()
        {
            return User.IsInRole("Patient");
        }
    }
}
