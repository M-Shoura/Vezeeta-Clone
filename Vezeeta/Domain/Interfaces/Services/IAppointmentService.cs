using Vezeeta.Application.DTOs.Appointments;

namespace Vezeeta.Domain.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();

        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByClinicAsync(int clinicId);

        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<int> CreateAppointmentAsync(CreateAppointmentDto createDto, string userId);

        Task<bool> UpdateAppointmentAsync(UpdateAppointmentDto updateDto, string userId);

        Task<bool> CancelAppointmentAsync(int appointmentId, string userId);

        Task<bool> CompleteAppointmentAsync(int appointmentId, string userId);

        Task<IEnumerable<(DateTime date, List<TimeSpan> availableSlots)>> GetAvailableSlotsAsync(
            int doctorId, 
            int clinicId, 
            DateTime startDate, 
            DateTime endDate,
            int slotDurationMinutes = 30);

        Task<bool> IsSlotAvailableAsync(
            int doctorId, 
            int clinicId, 
            DateTime appointmentDate, 
            TimeSpan startTime, 
            TimeSpan endTime);
    }
}
