using Application.DTOs.Prescriptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<PrescriptionDto>> GetAllAsync();
        Task<PrescriptionDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PrescriptionDto dto);
        Task<bool> UpdateAsync(int id, PrescriptionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
