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

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {

            _medicalRecordService = medicalRecordService;
        }
        public async Task<IActionResult> Index()
        {
            var records = await _medicalRecordService.GetAllAsync();

            return View(records);
        }
        [Authorize(Roles = "Admin,Doctor")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create(MedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _medicalRecordService.CreateAsync(dto);

            if (!result)
                return View(dto);

            return RedirectToAction(nameof(Index));

        }
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(
            int id,
            MedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _medicalRecordService.UpdateAsync(id, dto);

            if (!result)
                return View(dto);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int id)
        {
            await _medicalRecordService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
