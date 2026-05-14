using Application.DTOs.Profiles.Patients;

namespace Application.Interfaces.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<PatientProfileDto>>
            GetAllPatientsAsync();

        Task<PatientProfileDto?>
            GetPatientByIdAsync(string patientId);

        Task<IEnumerable<PatientProfileDto>>
            GetAvailablePatientsAsync();
    }
}
