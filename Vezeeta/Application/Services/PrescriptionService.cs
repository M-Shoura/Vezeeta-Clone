using Application.DTOs.Prescriptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PrescriptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PrescriptionDto>> GetAllAsync()
        {
            var prescriptions = await _unitOfWork
                .Repository<Prescription>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
        }

        public async Task<PrescriptionDto?> GetByIdAsync(int id)
        {
            var prescription = await _unitOfWork
                .Repository<Prescription>()
                .GetByIdAsync(id);

            if (prescription == null)
                return null;

            return _mapper.Map<PrescriptionDto>(prescription);
        }

        public async Task<bool> CreateAsync(PrescriptionDto dto)
        {
            var prescription = _mapper.Map<Prescription>(dto);

            prescription.PrescriptionDate = DateTime.UtcNow;

            await _unitOfWork
                .Repository<Prescription>()
                .AddAsync(prescription);

            await _unitOfWork.SaveChangesAsync();

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var item in dto.Items)
                {
                    item.PrescriptionId = prescription.Id;

                    var prescriptionItem =
                        _mapper.Map<PrescriptionItem>(item);

                    await _unitOfWork
                        .Repository<PrescriptionItem>()
                        .AddAsync(prescriptionItem);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateAsync(int id, PrescriptionDto dto)
        {
            var prescription = await _unitOfWork
                .Repository<Prescription>()
                .GetByIdAsync(id);

            if (prescription == null)
                return false;

            prescription.Notes = dto.Notes;
            prescription.PrescriptionDate = dto.PrescriptionDate;

            _unitOfWork
                .Repository<Prescription>()
                .Update(prescription);

            var oldItems = await _unitOfWork
                .Repository<PrescriptionItem>()
                .FindAllAsync(i => i.PrescriptionId == id);

            foreach (var item in oldItems)
            {
                _unitOfWork
                    .Repository<PrescriptionItem>()
                    .Delete(item);
            }

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var item in dto.Items)
                {
                    item.PrescriptionId = id;

                    var mappedItem =
                        _mapper.Map<PrescriptionItem>(item);

                    await _unitOfWork
                        .Repository<PrescriptionItem>()
                        .AddAsync(mappedItem);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var prescription = await _unitOfWork
                .Repository<Prescription>()
                .GetByIdAsync(id);

            if (prescription == null)
                return false;

            _unitOfWork
                .Repository<Prescription>()
                .Delete(prescription);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}