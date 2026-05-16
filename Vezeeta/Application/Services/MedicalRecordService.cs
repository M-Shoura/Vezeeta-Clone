using Application.DTOs.Medical_Records;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MedicalRecordService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetAllAsync()
        {
            var records = await _unitOfWork
                .Repository<MedicalRecord>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetAllByPatientIdAsync(string patientId)
        {
            var records = await _unitOfWork
                .Repository<MedicalRecord>()
                .FindAllAsync(r => r.PatientId == patientId);

            return _mapper.Map<IEnumerable<MedicalRecordDto>>(records);
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(int id)
        {
            var record = await _unitOfWork
                        .Repository<MedicalRecord>()
                        .GetByIdAsync(id);

            if (record == null)
                return null;

            return _mapper.Map<MedicalRecordDto>(record);
        }

        public async Task<bool> CreateAsync(MedicalRecordDto dto)
        {
            var record = _mapper.Map<MedicalRecord>(dto);

            await _unitOfWork
                .Repository<MedicalRecord>()
                .AddAsync(record);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(int id, MedicalRecordDto dto)
        {
            if (id != dto.Id)
                return false;

            var record = await _unitOfWork
                .Repository<MedicalRecord>()
                .GetByIdAsync(id);

            if (record == null)
                return false;

            _mapper.Map(dto, record);

            _unitOfWork
                .Repository<MedicalRecord>()
                .Update(record);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _unitOfWork
                .Repository<MedicalRecord>()
                .GetByIdAsync(id);

            if (record == null)
                return false;

            _unitOfWork
                .Repository<MedicalRecord>()
                .Delete(record);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
