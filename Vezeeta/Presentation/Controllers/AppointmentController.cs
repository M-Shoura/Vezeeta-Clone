using Vezeeta.Application.DTOs.Appointments;
using Vezeeta.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Vezeeta.Presentation.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        #region Appointment CRUD

        // GET: /Appointment
        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointments: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Appointment/Details/id
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

                if (appointment == null)
                    return NotFound();

                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Appointment/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new InvalidOperationException("User ID not found");

                await _appointmentService.CreateAppointmentAsync(dto, userId);

                TempData["Success"] = "Appointment created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating appointment: {ex.Message}";
                return View(dto);
            }
        }

        // GET: /Appointment/Edit/id
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

                if (appointment == null)
                    return NotFound();

                var updateDto = new UpdateAppointmentDto
                {
                    Id = appointment.Id,
                    AppointmentDate = appointment.AppointmentDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    ClinicId = appointment.ClinicId,
                    Notes = appointment.Notes
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Appointment/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAppointmentDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new InvalidOperationException("User ID not found");

                await _appointmentService.UpdateAppointmentAsync(dto, userId);

                TempData["Success"] = "Appointment updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating appointment: {ex.Message}";
                return View(dto);
            }
        }

        #endregion

        #region Appointment Actions

        // POST: /Appointment/Cancel/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new InvalidOperationException("User ID not found");

                await _appointmentService.CancelAppointmentAsync(id, userId);

                TempData["Success"] = "Appointment cancelled successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cancelling appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Appointment/Complete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new InvalidOperationException("User ID not found");

                await _appointmentService.CompleteAppointmentAsync(id, userId);

                TempData["Success"] = "Appointment completed successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error completing appointment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Availability

        // GET: /Appointment/GetAvailableSlots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, int clinicId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, clinicId, startDate, endDate);
                return Json(new { success = true, data = slots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Appointment/CheckSlotAvailability
        [HttpGet]
        public async Task<IActionResult> CheckSlotAvailability(int doctorId, int clinicId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var isAvailable = await _appointmentService.IsSlotAvailableAsync(doctorId, clinicId, appointmentDate, startTime, endTime);
                return Json(new { success = true, isAvailable });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Filtering

        // GET: /Appointment/ByDoctor/id
        public async Task<IActionResult> ByDoctor(int doctorId)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
                ViewBag.FilterType = "Doctor";
                ViewBag.FilterValue = doctorId;
                return View("Index", appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointments: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Appointment/ByPatient/id
        public async Task<IActionResult> ByPatient(int patientId)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
                ViewBag.FilterType = "Patient";
                ViewBag.FilterValue = patientId;
                return View("Index", appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointments: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Appointment/ByClinic/id
        public async Task<IActionResult> ByClinic(int clinicId)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByClinicAsync(clinicId);
                ViewBag.FilterType = "Clinic";
                ViewBag.FilterValue = clinicId;
                return View("Index", appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving appointments: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}
