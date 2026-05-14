using Application.DTOs.Drugs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IGenericRepository<Drug> _drugRepo;
        private readonly IMapper _mapper;

        public PrescriptionItemService(
            IGenericRepository<Drug> drugRepo,
            IMapper mapper)
        {
            _drugRepo = drugRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DrugFilterDto>> GetDrugsBySearchAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<DrugFilterDto>();

            var drugs = await _drugRepo.FindAllAsync(d =>
                d.Name.Contains(search) ||
                (d.GenericName != null && d.GenericName.Contains(search)));

            return _mapper.Map<IEnumerable<DrugFilterDto>>(drugs);
        }
    }
}
