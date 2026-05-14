using Application.DTOs.Drugs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IPrescriptionItemService
    {
        Task<IEnumerable<DrugFilterDto>> GetDrugsBySearchAsync(string search);
    }
}
