using Application.DTOs.Drugs;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class DrugController : Controller
    {
        private readonly IDrugsService _drugService;

        public DrugController(IDrugsService drugService)
        {
            _drugService = drugService;
        }
        public async Task<IActionResult> Index()
        {
            var drugs = await _drugService.GetAllAsync();

            return View(drugs);
        }
        public async Task<IActionResult> Details(int id)
        {
            var drug = await _drugService.GetByIdAsync(id);

            if (drug == null)
                return NotFound();

            return View(drug);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DrugDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _drugService.CreateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to create drug.");
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var drug = await _drugService.GetByIdAsync(id);

            if (drug == null)
                return NotFound();

            return View(drug);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DrugDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _drugService.UpdateAsync(id, dto);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to update drug.");
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _drugService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
