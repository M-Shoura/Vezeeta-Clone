using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientProfileService _patientService;

        public PatientController(
            IPatientProfileService patientService)
        {
            _patientService = patientService;
        }

        #region Patient CRUD

        // GET: /Patient
        public async Task<IActionResult> Index()
        {
            var patients =
                await _patientService
                    .GetAllPatientsAsync();

            return View(patients);
        }

        // GET: /Patient/Details/id
        public async Task<IActionResult> Details(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService
                    .GetPatientByIdAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // GET: /Patient/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Patient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PatientProfile patient)
        {
            if (!ModelState.IsValid)
                return View(patient);

            await _patientService
                .CreatePatientAsync(patient);

            TempData["Success"] =
                "Patient created successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patient/Edit/id
        public async Task<IActionResult> Edit(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService
                    .GetPatientByIdAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // POST: /Patient/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            PatientProfile patient)
        {
            if (id != patient.ApplicationUserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(patient);

            await _patientService
                .UpdatePatientAsync(patient);

            TempData["Success"] =
                "Patient updated successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patient/Delete/id
        public async Task<IActionResult> Delete(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var patient =
                await _patientService
                    .GetPatientByIdAsync(id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // POST: /Patient/Delete/id
        [HttpPost, ActionName("Delete")]
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
        public async Task<IActionResult> Profile()
        {
            var profile = await _patientService
                .GetCurrentPatientProfileAsync();

            return View(profile);
        }

        // GET: /Patient/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var profile = await _patientService
                .GetCurrentPatientProfileAsync();

            return View(profile);
        }

        // POST: /Patient/EditProfile
        [HttpPost]
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

        #region Appointments

        // GET: /Patient/Appointments
        public async Task<IActionResult> Appointments()
        {
            var appointments =
                await _patientService
                    .GetAllCurrentPatientAppointmentsAsync();

            return View(appointments);
        }

        #endregion
    }
}
