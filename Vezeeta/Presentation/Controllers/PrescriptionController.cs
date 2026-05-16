using Application.DTOs.Prescriptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPrescriptionItemService _prescriptionItemService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IPrescriptionItemService prescriptionItemService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _prescriptionService = prescriptionService;
            _prescriptionItemService = prescriptionItemService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Index()
        {
            var prescriptions = await _prescriptionService.GetAllAsync();

            if (User.IsInRole("Doctor"))
            {
                var doctorId = _currentUserService.UserId;
                var appointments = await _unitOfWork.Repository<Appointment>()
                    .FindAllAsync(a => a.DoctorId == doctorId && a.Prescription != null, includes: new[] { "Prescription" });

                var allowedPrescriptionIds = appointments.Select(a => a.Prescription!.Id).ToHashSet();
                prescriptions = prescriptions.Where(p => allowedPrescriptionIds.Contains(p.Id));
            }
            else if (User.IsInRole("Patient"))
            {
                var patientId = _currentUserService.UserId;
                var appointments = await _unitOfWork.Repository<Appointment>()
                    .FindAllAsync(a => a.PatientId == patientId && a.Prescription != null, includes: new[] { "Prescription" });

                var allowedPrescriptionIds = appointments.Select(a => a.Prescription!.Id).ToHashSet();
                prescriptions = prescriptions.Where(p => allowedPrescriptionIds.Contains(p.Id));
            }

            return View(prescriptions);
        }

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Details(int id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);
            if (prescription == null)
                return NotFound();

            if (User.IsInRole("Patient"))
            {
                var patientId = _currentUserService.UserId;
                if (prescription.AppointmentId == null)
                    return Forbid();

                var appointment = await _unitOfWork.Repository<Appointment>()
                    .FindAsync(a => a.Id == prescription.AppointmentId.Value && a.PatientId == patientId);

                if (appointment == null)
                    return Forbid();
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctorId = _currentUserService.UserId;
                if (prescription.AppointmentId == null)
                    return Forbid();

                var appointment = await _unitOfWork.Repository<Appointment>()
                    .FindAsync(a => a.Id == prescription.AppointmentId.Value && a.DoctorId == doctorId);

                if (appointment == null)
                    return Forbid();
            }

            return View(prescription);
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment = await GetDoctorBookedAppointmentAsync(appointmentId);

            if (appointment == null)
                return NotFound();

            var existingPrescription = await _prescriptionService.GetByAppointmentIdAsync(appointmentId);

            if (existingPrescription != null)
                return RedirectToAction(nameof(Edit), new { id = existingPrescription.Id });

            ViewBag.AppointmentInfo = GetAppointmentInfo(appointment);

            return View(new PrescriptionDto
            {
                AppointmentId = appointmentId,
                PrescriptionDate = System.DateTime.Today
            });
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);

            if (prescription == null || prescription.AppointmentId == null)
                return NotFound();

            var appointment = await GetDoctorBookedAppointmentAsync(prescription.AppointmentId.Value);

            if (appointment == null)
                return NotFound();

            ViewBag.AppointmentInfo = GetAppointmentInfo(appointment);

            return View("Create", prescription);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Save([FromBody] PrescriptionDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            if (dto.AppointmentId == null)
                return Json(new { success = false, message = "Appointment is required" });

            var appointment = await GetDoctorBookedAppointmentAsync(dto.AppointmentId.Value);

            if (appointment == null)
                return Json(new { success = false, message = "Prescription can only be added for your booked appointments" });

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

        private async Task<Appointment?> GetDoctorBookedAppointmentAsync(int appointmentId)
        {
            var doctorId = _currentUserService.UserId;

            if (string.IsNullOrWhiteSpace(doctorId))
                return null;

            return await _unitOfWork.Repository<Appointment>()
                .FindAsync(a =>
                    a.Id == appointmentId
                    && a.DoctorId == doctorId
                    && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed),
                    includes: new[]
                    {
                        "Patient",
                        "Patient.ApplicationUser",
                        "Clinic"
                    });
        }

        private static string GetAppointmentInfo(Appointment appointment)
        {
            return $"{appointment.Patient.ApplicationUser.FullName} - {appointment.Clinic.Name} - {appointment.AppointmentDate:dd MMM yyyy}, {appointment.StartTime:hh\\:mm}";
        }
    }
}
