using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.ViewModels;

namespace Presentation.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public DoctorController(
            IDoctorService doctorService,
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _doctorService = doctorService;
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        #region Doctor CRUD

        // GET: /Doctor
        [AllowAnonymous]
        public async Task<IActionResult> Index(
    string? name,
    string? specialization)
        {
            var doctors =
                await _doctorService
                    .SearchDoctorsAsync(
                        name,
                        specialization);

            var clinics =
                await _doctorService
                    .GetAllClinicsAsync();

            ViewBag.Clinics = clinics;

            ViewBag.Specializations =
                new SelectList(
                    await _doctorService.GetAllSpecializationsAsync(),
                    specialization);

            ViewBag.Name = name;
            ViewBag.Specialization = specialization;

            return View(doctors);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(
    string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var doctor =
                await _doctorService
                    .GetDoctorDetailsAsync(id);

            if (doctor == null)
                return NotFound();

            var availableSlots =
                new List<Application.DTOs.Appointments.AvailableSlotDto>();

            for (var i = 0; i < 14; i++)
            {
                var date = DateTime.Today.AddDays(i);

                var slotsForDate =
                    await _doctorService
                        .GetAvailableSlotsAsync(
                            id,
                            date);

                availableSlots.AddRange(slotsForDate);
            }

            ViewBag.AvailableSlots =
                availableSlots
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.StartTime);

            ViewBag.Appointments =
                await _doctorService
                    .GetDoctorAppointmentsAsync(id);

            return View(doctor);
        }

        // GET: /Doctor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Doctor/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DoctorProfile doctor)
        {
            if (!ModelState.IsValid)
                return View(doctor);

            await _doctorService
                .CreateDoctorAsync(doctor);

            TempData["Success"] =
                "Doctor created successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();
            if (!CanManageDoctor(id))
                return Forbid();

            var doctor =
                await _doctorService
                    .GetDoctorByIdAsync(id);

            if (doctor == null)
                return NotFound();

            var model = new EditDoctorViewModel
            {
                ApplicationUserId = doctor.Id,
                FullName = doctor.FullName,
                ProfilePicture = doctor.ProfilePicture,
                Specialization = doctor.Specialization,
                YearsOfExperience = doctor.YearsOfExperience,
                Bio = doctor.Bio,
                Qualification = doctor.Qualification,
                IsAvailable = doctor.IsAvailable,
                LicenseNumber = doctor.LicenseNumber
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    string id,
    EditDoctorViewModel model)
        {
            if (id != model.ApplicationUserId)
                return BadRequest();
            if (!CanManageDoctor(id))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var clinics = await _doctorService.GetAllClinicsAsync();
                ViewBag.Clinics = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(clinics, "Id", "Name");
                return View(model);
            }
            var doctor = new DoctorProfile
            {
                ApplicationUserId = model.ApplicationUserId,
                Specialization = model.Specialization,
                YearsOfExperience = model.YearsOfExperience,
                Bio = model.Bio,
                Qualification = model.Qualification,
                IsAvailable = model.IsAvailable,
                LicenseNumber = model.LicenseNumber
            };

            await _doctorService
                .UpdateDoctorAsync(doctor);

            TempData["Success"] =
                "Doctor updated successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Doctor/Delete/id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var doctor =
                await _doctorService
                    .GetDoctorByIdAsync(id);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // POST: /Doctor/Delete/id
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            await _doctorService
                .DeleteDoctorAsync(id);

            TempData["Success"] =
                "Doctor deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Availability

        // GET: /Doctor/AvailableDoctors
        [AllowAnonymous]
        public async Task<IActionResult>
            AvailableDoctors()
        {
            var doctors =
                await _doctorService
                    .GetAvailableDoctorsAsync();

            return View(doctors);
        }

        // POST: /Doctor/ToggleAvailability/id
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ToggleAvailability(
                string id,
                bool isAvailable)
        {
            await _doctorService
                .UpdateDoctorAvailabilityAsync(
                    id,
                    isAvailable);

            TempData["Success"] =
                "Availability updated successfully";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        #endregion

        #region Search

        // GET: /Doctor/Search
        [AllowAnonymous]
        public IActionResult Search()
        {
            return View();
        }

        // POST: /Doctor/SearchBySpecialization
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult>
            SearchBySpecialization(
                string specialization)
        {
            if (string.IsNullOrWhiteSpace(
                specialization))
            {
                return View(
                    "SearchResults",
                    new List<object>());
            }

            var doctors =
                await _doctorService
                    .GetDoctorsBySpecializationAsync(
                        specialization);

            ViewBag.Specialization =
                specialization;

            return View(
                "SearchResults",
                doctors);
        }

        #endregion

        #region Doctor Clinics

        // GET: /Doctor/AssignClinic/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AssignClinic(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();
            if (!CanManageDoctor(id))
                return Forbid();

            ViewBag.DoctorId = id;

            var clinics = await _doctorService.GetAllClinicsAsync();
            ViewBag.Clinics = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                clinics, "Id", "Name");

            return View();
        }

        // POST: /Doctor/AssignClinic
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AssignClinic(
                string doctorId,
                int clinicId,
                decimal consultationFee)
        {
            if (!CanManageDoctor(doctorId))
                return Forbid();

            await _doctorService
                .AssignDoctorToClinicAsync(
                    doctorId,
                    clinicId,
                    consultationFee);

            TempData["Success"] =
                "Clinic assigned successfully";

            return RedirectToAction(
                nameof(Dashboard),
                new { id = doctorId });
        }

        // POST: /Doctor/UpdateConsultationFee
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConsultationFee(
            string doctorId,
            int clinicId,
            decimal consultationFee)
        {
            if (!CanManageDoctor(doctorId))
                return Forbid();

            try
            {
                await _doctorService
                    .UpdateConsultationFeeAsync(doctorId, clinicId, consultationFee);
                TempData["Success"] = "Consultation fee updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard), new { id = doctorId });
        }

        #endregion

        #region Doctor Schedule

        // GET: /Doctor/Schedules/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult>
            Schedules(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();
            if (!CanManageDoctor(id))
                return Forbid();

            var schedules =
                await _doctorService
                    .GetDoctorSchedulesAsync(id);

            ViewBag.DoctorId = id;

            return View(schedules);
        }

        // GET: /Doctor/AddSchedule?doctorId=...
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddSchedule(
            string doctorId)
        {
            if (!CanManageDoctor(doctorId))
                return Forbid();

            ViewBag.DoctorId = doctorId;

            await PopulateDoctorClinicSelectListAsync(doctorId);

            var model = new Presentation.ViewModels.CreateDoctorScheduleViewModel
            {
                DoctorId = doctorId,
                SlotDurationMinutes = 30
            };

            return View(model);
        }

        // POST: /Doctor/AddSchedule
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddSchedule(
                [FromForm] Presentation.ViewModels.CreateDoctorScheduleViewModel schedule)
        {
            // Ensure DoctorId is preserved for the view when returning
            ViewBag.DoctorId = schedule?.DoctorId;

            if (schedule == null)
                return BadRequest();
            if (!CanManageDoctor(schedule.DoctorId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateDoctorClinicSelectListAsync(schedule.DoctorId, schedule.ClinicId);
                return View(schedule);
            }

            var doctorSchedule = new DoctorSchedule
            {
                ClinicId = schedule.ClinicId,
                Day = schedule.Day,
                DoctorId = schedule.DoctorId,
                EndTime = schedule.EndTime,
                SlotDurationMinutes = schedule.SlotDurationMinutes,
                StartTime = schedule.StartTime,
                IsActive = true
            };

            try
            {
                await _doctorService.CreateDoctorScheduleAsync(doctorSchedule);
                TempData["Success"] = "Schedule added successfully";
                return RedirectToAction(nameof(Schedules), new { id = schedule.DoctorId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDoctorClinicSelectListAsync(schedule.DoctorId, schedule.ClinicId);
                return View(schedule);
            }
        }

        // GET: /Doctor/DeleteSchedule/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult>
            DeleteSchedule(int id)
        {
            var schedule =
                await _doctorService
                    .GetScheduleByIdAsync(id);

            if (schedule == null)
                return NotFound();
            if (!CanManageDoctor(schedule.DoctorId))
                return Forbid();

            return View(schedule);
        }

        // POST: /Doctor/DeleteSchedule/id
        [HttpPost, ActionName("DeleteSchedule")]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteScheduleConfirmed(int id)
        {
            var schedule = await _doctorService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();
            if (!CanManageDoctor(schedule.DoctorId))
                return Forbid();

            var doctorId =
                await _doctorService
                    .DeleteScheduleAsync(id);

            TempData["Success"] =
                "Schedule deleted successfully";

            return RedirectToAction(
                nameof(Schedules),
                new { id = doctorId });
        }

        #endregion

        #region Dashboard

        // GET: /Doctor/Dashboard/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult>
            Dashboard(string? id)
        {
            var doctorId = string.IsNullOrWhiteSpace(id) ? _currentUserService.UserId : id;

            if (string.IsNullOrWhiteSpace(doctorId))
                return Challenge();

            if (User.IsInRole("Doctor") && doctorId != _currentUserService.UserId)
                return Forbid();

            var model = await _dashboardService.GetDoctorDashboardAsync(doctorId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        #endregion

        private async Task PopulateDoctorClinicSelectListAsync(
            string doctorId,
            int? selectedClinicId = null)
        {
            var clinics = await _doctorService.GetClinicsForDoctorAsync(doctorId);

            ViewBag.Clinics = new SelectList(
                clinics,
                "Id",
                "Name",
                selectedClinicId);
        }

        private bool CanManageDoctor(string? doctorId)
        {
            if (User.IsInRole("Admin"))
                return true;

            return User.IsInRole("Doctor")
                && !string.IsNullOrWhiteSpace(doctorId)
                && doctorId == _currentUserService.UserId;
        }

    }
}
