using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Presentation.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentController(
            IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _unitOfWork.Repository<Appointment>().FindAllAsync(
                a => true,
                includes: new[]
                {
                    "Doctor",
                    "Doctor.ApplicationUser",
                    "Patient",
                    "Patient.ApplicationUser",
                    "Clinic",
                    "Payment"
                });

            if (User.IsInRole("Doctor"))
            {
                var userId = _httpContextAccessor.HttpContext?.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                appointments = appointments.Where(a => a.DoctorId == userId);
            }

            return View(appointments.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime));
        }

        // GET
        [Authorize(Roles = "Admin,Doctor")]
        public IActionResult CreateAvailableSlot(
            string doctorId)
        {
            ViewBag.DoctorId = doctorId;

            return View();
        }

        [Authorize(Roles = "Admin,Doctor")]
        public IActionResult AddSchedule(string doctorId)
        {
            ViewBag.DoctorId = doctorId;
            return View();
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book([FromForm] Presentation.ViewModels.BookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for conflicting appointments
            var existing = await _unitOfWork.Repository<Appointment>().FindAllAsync(a =>
                a.DoctorId == model.DoctorId && a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.StartTime == model.StartTime);

            if (existing.Any())
            {
                return BadRequest("Selected slot is already booked");
            }

            var appointment = new Appointment
            {
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                ClinicId = model.ClinicId,
                Status = Domain.Enums.AppointmentStatus.Confirmed,
                Notes = model.Notes
            };

            // set patient id from claims if available
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var uid = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                appointment.PatientId = uid;
            }

            if (string.IsNullOrWhiteSpace(appointment.PatientId))
                return Challenge();

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToAction(
                actionName: "Create",
                controllerName: "Payment",
                routeValues: new
                {
                    appointmentId = appointment.Id,
                    autoCheckout = true
                });
        }
    }
}
