using Application.DTOs.Doctors;

namespace Application.Interfaces.Repositories
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<DoctorDto>>
            GetAllDoctorsAsync();

        Task<DoctorDto?>
            GetDoctorByIdAsync(string doctorId);

        Task<IEnumerable<DoctorDto>>
            GetAvailableDoctorsAsync();

        Task<IEnumerable<DoctorDto>>
            GetDoctorsBySpecializationAsync(
                string specialization);
    }
}
