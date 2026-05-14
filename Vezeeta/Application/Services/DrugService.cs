using Application.DTOs.Drugs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class DrugService : IDrugsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DrugService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> CreateAsync(DrugDto dto)
        {
            var mappedDrug = _mapper.Map<Drug>(dto);

            await _unitOfWork
                .Repository<Drug>()
                .AddAsync(mappedDrug);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var drug = await _unitOfWork
                .Repository<Drug>()
                .FindAsync(d => d.Id == id);

            if (drug == null)
            {
                return false;
            }

            _unitOfWork
                .Repository<Drug>()
                .Delete(drug);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<DrugPagedResultDto> GetAllAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Repository<Drug>().GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.Name.Contains(search) ||
                    (d.GenericName != null && d.GenericName.Contains(search)) ||
                    (d.Manufacturer != null && d.Manufacturer.Contains(search)));
            }

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new DrugPagedResultDto
            {
                Items = _mapper.Map<IEnumerable<DrugDto>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Search = search
            };
        }

        public async Task<DrugDto?> GetByIdAsync(int id)
        {
            var drug = await _unitOfWork
                .Repository<Drug>()
                .FindAsync(d => d.Id == id);

            return _mapper.Map<DrugDto?>(drug);
        }

        public async Task<bool> UpdateAsync(int id, DrugDto dto)
        {
            if (id != dto.Id)
                return false;

            var drug = await _unitOfWork .Repository<Drug>().FindAsync(d => d.Id == id);

            if (drug == null)
                return false;
            

            _mapper.Map(dto, drug);

            _unitOfWork.Repository<Drug>().Update(drug);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}