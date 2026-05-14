using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.ViewModels.Reviews;

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
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return View(reviews);
        }

        // GET: /Review/DoctorReviews/id
        public async Task<IActionResult> DoctorReviews(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var reviews = await _reviewService.GetDoctorReviewsAsync(id);

            return View(reviews);
        }

        // GET: /Review/PatientReviews/id
        public async Task<IActionResult> PatientReviews(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var reviews = await _reviewService.GetPatientReviewsAsync(id);

            return View(reviews);
        }

        // GET: /Review/Details/id
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            return View(review);
        }

        // GET: /Review/Create
        public IActionResult Create(int appointmentId = 0)
        {
            var vm = new ReviewCreateViewModel
            {
                AppointmentId = appointmentId,
            };

            return View(vm);
        }

        // POST: /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(vm.AppointmentId);
            if (appointment == null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Appointment not found.");
                return View(vm);
            }

            var existingReview = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.AppointmentId == vm.AppointmentId);
            if (existingReview != null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "A review already exists for this appointment.");
                return View(vm);
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
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewEditViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(vm.AppointmentId);
            if (appointment == null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Appointment not found.");
                return View(vm);
            }

            var conflictingReview = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.AppointmentId == vm.AppointmentId && r.Id != vm.Id);
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
                AppointmentId = vm.AppointmentId,
                DoctorId = appointment.DoctorId!,
                PatientId = appointment.PatientId!
            };

            await _reviewService.UpdateReviewAsync(review);

            TempData["Success"] = "Review updated successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Review/Delete/id
        public async Task<IActionResult> Delete(int id)
        {
            var dto = await _reviewService.GetReviewByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // POST: /Review/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reviewService.DeleteReviewAsync(id);

            TempData["Success"] = "Review deleted successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
