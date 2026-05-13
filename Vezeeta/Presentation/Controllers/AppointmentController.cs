using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDoctorService _doctorService;

        public AppointmentController(
            IUnitOfWork unitOfWork, IDoctorService doctorService)
        {
            _unitOfWork = unitOfWork;
            _doctorService = doctorService;
        }

        // GET
        public IActionResult CreateAvailableSlot(
            string doctorId)
        {
            ViewBag.DoctorId = doctorId;

            return View();
        }

        public IActionResult AddSchedule(
    string doctorId)
        {
            ViewBag.DoctorId = doctorId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
    AddSchedule(
        DoctorSchedule schedule)
        {
            if (!ModelState.IsValid)
                return View(schedule);

            await _doctorService
                .CreateDoctorScheduleAsync(
                    schedule);

            TempData["Success"] =
                "Schedule added successfully";

            return RedirectToAction(
    "Details",
    "Doctor",
    new { id = schedule.DoctorId });
        }
    }
}
