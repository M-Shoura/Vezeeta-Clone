using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class ClinicController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClinicController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var clinics = await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync();

            return View(clinics.OrderBy(c => c.Name));
        }

        public async Task<IActionResult> Details(int id)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FindAsync(
                    c => c.Id == id,
                    includes: new[]
                    {
                        "DoctorClinics",
                        "DoctorClinics.Doctor",
                        "DoctorClinics.Doctor.ApplicationUser"
                    });

            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        public IActionResult Create()
        {
            return View(new Clinic { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Clinic clinic)
        {
            if (!ModelState.IsValid)
                return View(clinic);

            await _unitOfWork
                .Repository<Clinic>()
                .AddAsync(clinic);

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Clinic created successfully";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FindAsync(c => c.Id == id);

            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Clinic clinic)
        {
            if (id != clinic.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(clinic);

            var existingClinic = await _unitOfWork
                .Repository<Clinic>()
                .FindAsync(c => c.Id == id);

            if (existingClinic == null)
                return NotFound();

            existingClinic.Name = clinic.Name;
            existingClinic.Address = clinic.Address;
            existingClinic.PhoneNumber = clinic.PhoneNumber;
            existingClinic.Email = clinic.Email;
            existingClinic.Description = clinic.Description;
            existingClinic.IsActive = clinic.IsActive;
            existingClinic.UpdatedAt = DateTime.UtcNow;

            _unitOfWork
                .Repository<Clinic>()
                .Update(existingClinic);

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Clinic updated successfully";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FindAsync(c => c.Id == id);

            if (clinic == null)
                return NotFound();

            return View(clinic);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .FindAsync(c => c.Id == id);

            if (clinic == null)
                return NotFound();

            _unitOfWork
                .Repository<Clinic>()
                .Delete(clinic);

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Clinic deleted successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
