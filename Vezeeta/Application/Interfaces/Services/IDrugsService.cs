using Application.DTOs.Drugs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IDrugsService
    {
        Task<IEnumerable<DrugDto>> GetAllAsync();

        Task<DrugDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(DrugDto dto);

        Task<bool> UpdateAsync(int id, DrugDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
