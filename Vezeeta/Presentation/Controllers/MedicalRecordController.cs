using Application.DTOs.Medical_Records;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly ICurrentUserService _currentUserService;

        public MedicalRecordController(
            IMedicalRecordService medicalRecordService,
            ICurrentUserService currentUserService)
        {
            _medicalRecordService = medicalRecordService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(string? patientId = null)
        {
            if (IsPatientUser())
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Challenge();

                var records = await _medicalRecordService.GetAllByPatientIdAsync(currentUserId);
                return View(records);
            }

            if (IsDoctorUser() && !string.IsNullOrWhiteSpace(patientId))
            {
                var records = await _medicalRecordService.GetAllByPatientIdAsync(patientId);
                return View(records);
            }

            var allRecords = await _medicalRecordService.GetAllAsync();
            return View(allRecords);
        }

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public IActionResult Create()
        {
            var dto = new MedicalRecordDto
            {
                DiagnosedDate = DateTime.Today
            };

            if (IsPatientUser())
            {
                dto.PatientId = GetCurrentUserId() ?? string.Empty;
            }

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Create(MedicalRecordDto dto)
        {
            if (IsPatientUser())
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Challenge();

                dto.PatientId = currentUserId;
            }

            if (!ModelState.IsValid)
                return View(dto);

            var result = await _medicalRecordService.CreateAsync(dto);
            if (!result)
                return View(dto);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);
            if (record == null)
                return NotFound();

            if (IsPatientUser() && record.PatientId != GetCurrentUserId())
                return Forbid();

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Edit(int id, MedicalRecordDto dto)
        {
            if (id != dto.Id)
                return NotFound();

            var existing = await _medicalRecordService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (IsPatientUser() && existing.PatientId != GetCurrentUserId())
                return Forbid();

            if (IsPatientUser())
            {
                dto.PatientId = GetCurrentUserId()!;
            }

            if (!ModelState.IsValid)
                return View(dto);

            var result = await _medicalRecordService.UpdateAsync(id, dto);
            if (!result)
                return View(dto);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _medicalRecordService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (IsPatientUser() && existing.PatientId != GetCurrentUserId())
                return Forbid();

            await _medicalRecordService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private string? GetCurrentUserId()
        {
            return _currentUserService.UserId;
        }

        private bool IsPatientUser()
        {
            return User.IsInRole("Patient");
        }

        private bool IsDoctorUser()
        {
            return User.IsInRole("Doctor");
        }
    }
}
