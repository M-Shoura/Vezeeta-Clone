using Vezeeta.Application.DTOs.Appointments;
using Vezeeta.Domain.Entities;

namespace Vezeeta.Domain.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();

        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByClinicAsync(int clinicId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<Appointment?> GetAppointmentEntityByIdAsync(int id);

        Task AddAppointmentAsync(Appointment appointment);

        Task UpdateAppointmentAsync(Appointment appointment);

        Task DeleteAppointmentAsync(int id);

        Task<bool> IsSlotAvailableAsync(int doctorId, int clinicId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime);
    }
}
