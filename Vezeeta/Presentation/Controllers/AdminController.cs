using Application.Interfaces.Services;
using Application.DTOs.Profiles.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IPatientProfileService _patientService;

        public AdminController(
            IDashboardService dashboardService,
            IPatientProfileService patientService)
        {
            _dashboardService = dashboardService;
            _patientService = patientService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dashboard = await _dashboardService.GetAdminDashboardAsync(cancellationToken);
            return View(dashboard);
        }

        // ==================== PATIENT MANAGEMENT ====================

        public async Task<IActionResult> EditPatient(string id, string? returnUrl)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var patient = await _patientService.GetPatientByUserIdAsync(id);
            if (patient == null)
                return NotFound();

            ViewData["ReturnUrl"] = returnUrl;

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(string id, PatientDto dto, string? returnUrl)
        {
            if (string.IsNullOrEmpty(id) || id != dto.ApplicationUserId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(dto);
            }

            var patient = new Domain.Entities.PatientProfile
            {
                ApplicationUserId = dto.ApplicationUserId,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                BloodType = dto.BloodType,
                EmergencyContactName = dto.EmergencyContactName ?? string.Empty,
                EmergencyContactPhone = dto.EmergencyContactPhone ?? string.Empty
            };

            await _patientService.UpdatePatientAsync(patient);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Patient", new { id });
        }
    }
}
