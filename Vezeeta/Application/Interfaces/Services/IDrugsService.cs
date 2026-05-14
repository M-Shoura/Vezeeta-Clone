using Application.DTOs.Drugs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IDrugsService
    {
        Task<DrugPagedResultDto> GetAllAsync(string? search, int pageNumber, int pageSize);

        Task<DrugDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(DrugDto dto);

        Task<bool> UpdateAsync(int id, DrugDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
