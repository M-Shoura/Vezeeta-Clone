using Application.DTOs.Profiles.Patients;
using Application.DTOs.Profiles;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IPatientProfileService
    {
        #region Patient Queries

        Task<IEnumerable<PatientDto>>
            GetAllPatientsAsync();

        Task<PatientDto?>
            GetPatientByIdAsync(string patientId);

        Task<PatientDto?>
            GetPatientByUserIdAsync(string userId);

        #endregion

        #region Patient CRUD

        Task CreatePatientAsync(
            PatientProfile patient);

        Task UpdatePatientAsync(
            PatientProfile patient);

        Task DeletePatientAsync(
            string patientId);

        #endregion

        #region Patient Profile

        Task<PatientProfileDto> GetCurrentPatientProfileAsync();

        Task<int> UpdatePatientProfileAsync(PatientProfileDto model);

        

        #endregion
    }
}