using Application.DTOs.Appointments;
using Application.DTOs.Doctors;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<AvailableSlotDto>>
    GetAvailableSlotsAsync(
        string doctorId,
        DateTime date);

        #region Doctor Queries
        Task<DoctorDetailsDto?>
    GetDoctorDetailsAsync(string doctorId);
        Task<IEnumerable<DoctorDto>>
            GetAllDoctorsAsync();

        Task<DoctorDto?>
            GetDoctorByIdAsync(string doctorId);

        Task<IEnumerable<DoctorDto>>
            GetAvailableDoctorsAsync();

        Task<IEnumerable<DoctorDto>>
            GetDoctorsBySpecializationAsync(
                string specialization);

        #endregion

        #region Doctor CRUD

        Task CreateDoctorAsync(
            DoctorProfile doctor);

        Task UpdateDoctorAsync(
            DoctorProfile doctor);

        Task DeleteDoctorAsync(
            string doctorId);

        #endregion

        #region Availability

        Task UpdateDoctorAvailabilityAsync(
            string doctorId,
            bool isAvailable);

        #endregion

        #region Doctor Clinics

        Task AssignDoctorToClinicAsync(
            string doctorId,
            int clinicId,
            decimal fee);

        #endregion

        #region Doctor Schedules

        Task CreateDoctorScheduleAsync(
            DoctorSchedule schedule);

        Task<IEnumerable<DoctorSchedule>>
            GetDoctorSchedulesAsync(
                string doctorId);

        Task<DoctorSchedule?>
            GetScheduleByIdAsync(
                int scheduleId);

        Task<string>
            DeleteScheduleAsync(
                int scheduleId);

        Task<IEnumerable<Domain.Entities.Appointment>>
            GetDoctorAppointmentsAsync(string doctorId);

        Task<IEnumerable<Domain.Entities.DoctorClinic>>
            GetDoctorClinicsAsync(string doctorId);

        Task<IEnumerable<Clinic>>
            GetClinicsForDoctorAsync(string doctorId);

        Task UpdateConsultationFeeAsync(
            string doctorId,
            int clinicId,
            decimal newFee);

        #endregion


        Task<IEnumerable<DoctorDto>>
    SearchDoctorsAsync(
        string? name,
        string? specialization);

        Task<IEnumerable<Clinic>>
            GetAllClinicsAsync();

        Task<IEnumerable<string>>
            GetAllSpecializationsAsync();
    }
}
