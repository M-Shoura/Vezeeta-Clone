using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Reviews;

namespace Presentation.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
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
        public IActionResult Create(int appointmentId = 0, string doctorId = null, string patientId = null)
        {
            var vm = new ReviewCreateViewModel
            {
                AppointmentId = appointmentId,
                DoctorId = doctorId ?? string.Empty,
                PatientId = patientId ?? string.Empty
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

            var review = new Review
            {
                Rating = vm.Rating,
                Comment = vm.Comment,
                AppointmentId = vm.AppointmentId,
                DoctorId = vm.DoctorId,
                PatientId = vm.PatientId
            };

            await _reviewService.AddReviewAsync(review);

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

            var review = new Review
            {
                Id = vm.Id,
                Rating = vm.Rating,
                Comment = vm.Comment,
                AppointmentId = vm.AppointmentId,
                DoctorId = vm.DoctorId,
                PatientId = vm.PatientId
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
