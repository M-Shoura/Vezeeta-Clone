using Application.DTOs.Doctors;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IDoctorService
    {
        #region Doctor Queries

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

        #endregion
    }
}