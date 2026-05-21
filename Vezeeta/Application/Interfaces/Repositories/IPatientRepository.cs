using Application.DTOs.Profiles.Patients;

namespace Application.Interfaces.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<PatientDto>>
            GetAllPatientsAsync();

        Task<PatientDto?>
            GetPatientByIdAsync(string patientId);

        Task<IEnumerable<PatientDto>>
            GetAvailablePatientsAsync();
    }
}
