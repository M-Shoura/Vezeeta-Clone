using Application.DTOs.Profiles.Patients;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels;

namespace Presentation.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientProfileService _patientService;
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public PatientController(
            IPatientProfileService patientService,
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _patientService = patientService;
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        #region Patient CRUD

        // GET: /Patient
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var patients =
                await _patientService
                    .GetAllPatientsAsync();

            return View(patients);
        }

        // GET: /Patient/Details/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService.GetPatientByUserIdAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // GET: /Patient/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new CreatePatientViewModel());
        }

        // POST: /Patient/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = new PatientProfile
            {
                ApplicationUserId = model.ApplicationUserId,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                BloodType = model.BloodType,
                EmergencyContactName = model.EmergencyContactName ?? string.Empty,
                EmergencyContactPhone = model.EmergencyContactPhone ?? string.Empty
            };

            await _patientService
                .CreatePatientAsync(patient);

            TempData["Success"] =
                "Patient created successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patient/Edit/id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            string id,
            string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService.GetPatientByUserIdAsync(id);

            if (patient == null)
                return NotFound();

            ViewData["ReturnUrl"] = returnUrl;

            return View(patient);
        }

        // POST: /Patient/Edit/id
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            PatientDto model,
            string? returnUrl)
        {
            if (id != model.ApplicationUserId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var patient = new PatientProfile
            {
                ApplicationUserId = model.ApplicationUserId,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                BloodType = model.BloodType,
                EmergencyContactName = model.EmergencyContactName ?? string.Empty,
                EmergencyContactPhone = model.EmergencyContactPhone ?? string.Empty
            };

            await _patientService
                .UpdatePatientAsync(patient);

            TempData["Success"] =
                "Patient updated successfully";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patient/Delete/id
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService.GetPatientByUserIdAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // POST: /Patient/Delete/id
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            await _patientService
                .DeletePatientAsync(id);

            TempData["Success"] =
                "Patient deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Patient Profile

        // GET: /Patient/Profile
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Profile()
        {
            var profile = await _patientService
                .GetCurrentPatientProfileAsync();

            return View(profile);
        }

        // GET: /Patient/EditProfile
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditProfile()
        {
            var profile = await _patientService
                .GetCurrentPatientProfileAsync();

            return View(profile);
        }

        // POST: /Patient/EditProfile
        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            Application.DTOs.Profiles.Patients.PatientProfileDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _patientService
                .UpdatePatientProfileAsync(model);

            TempData["Success"] =
                "Profile updated successfully";

            return RedirectToAction(nameof(Profile));
        }

        #endregion



        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
                return Challenge();

            var dashboard = await _dashboardService.GetPatientDashboardAsync(
                _currentUserService.UserId,
                cancellationToken);

            if (dashboard == null)
                return NotFound();

            return View(dashboard);
        }
    }
}
