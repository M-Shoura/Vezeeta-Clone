using Application.DTOs.Profiles.Patients;
using Application.DTOs.Profiles;

namespace Application.Interfaces.Services
{
    public interface IPatientProfileService
    {
        Task<PatientProfileDto> GetCurrentPatientProfileAsync();
        Task<int> UpdatePatientProfileAsync(PatientProfileDto model);
        Task ChangePasswordAsync(ChangePasswordDto model);
        Task<PatientAppointmentsDto> GetAllCurrentPatientAppointmentsAsync();
    }
}