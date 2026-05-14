using Vezeeta.Application.DTOs.Appointments;
using Vezeeta.Domain.Entities;
using Vezeeta.Domain.Interfaces.Repositories;
using Vezeeta.Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Vezeeta.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.Id == id)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            var doctorIdStr = doctorId.ToString();
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.DoctorId == doctorIdStr)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId)
        {
            var patientIdStr = patientId.ToString();
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.PatientId == patientIdStr)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByClinicAsync(int clinicId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.ClinicId == clinicId)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    DoctorId = int.Parse(a.DoctorId ?? "0"),
                    DoctorName = a.Doctor != null ? a.Doctor.ApplicationUser!.FullName : null,
                    PatientId = int.Parse(a.PatientId ?? "0"),
                    PatientName = a.Patient != null ? a.Patient.ApplicationUser!.FullName : null,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    Notes = a.Notes,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    LastModifiedOn = a.LastModifiedOn,
                    LastModifiedBy = a.LastModifiedBy
                })
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentEntityByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await Task.CompletedTask;
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await GetAppointmentEntityByIdAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }
        }

        public async Task<bool> IsSlotAvailableAsync(int doctorId, int clinicId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            var doctorIdStr = doctorId.ToString();
            
            var conflictingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorIdStr
                    && a.ClinicId == clinicId
                    && a.AppointmentDate == appointmentDate
                    && a.Status != Domain.Enums.AppointmentStatus.Cancelled)
                .ToListAsync();

            // Check if there's any overlap
            return !conflictingAppointments.Any(a => 
                (startTime < a.EndTime && endTime > a.StartTime));
        }
    }
}
