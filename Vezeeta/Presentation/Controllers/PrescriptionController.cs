using Application.DTOs.Prescriptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPrescriptionItemService _prescriptionItemService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IPrescriptionItemService prescriptionItemService)
        {
            _prescriptionService = prescriptionService;
            _prescriptionItemService = prescriptionItemService;
        }

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Index()
        {
            var prescriptions = await _prescriptionService.GetAllAsync();
            return View(prescriptions);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Save([FromBody] PrescriptionDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            bool result;

            if (dto.Id == 0)
            {
                result = await _prescriptionService.CreateAsync(dto);
            }
            else
            {
                result = await _prescriptionService.UpdateAsync(dto.Id, dto);
            }

            if (!result)
                return Json(new { success = false, message = "Failed to save prescription" });

            return Json(new { success = true, message = "Prescription saved successfully" });
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _prescriptionService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Failed to delete prescription" });

            return Json(new { success = true, message = "Prescription deleted successfully" });
        }
    
        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDrugsBySearch(string search)
        {
            var drugs = await _prescriptionItemService.GetDrugsBySearchAsync(search);
            return Json(drugs);
        }
    }
}
