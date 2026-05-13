using Application.DTOs.Reviews;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewController(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET
        public IActionResult Create(string doctorId)
        {
            ViewBag.DoctorId = doctorId;

            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                AppointmentId = dto.AppointmentId,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Review added successfully";

            return RedirectToAction("Details", "Doctor", new { id = dto.DoctorId });
        }
    }
}
