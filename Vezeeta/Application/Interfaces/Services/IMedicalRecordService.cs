using Application.DTOs.Medical_Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IMedicalRecordService
    {
        Task<IEnumerable<MedicalRecordDto>> GetAllAsync();

        Task<IEnumerable<MedicalRecordDto>> GetAllByPatientIdAsync(string patientId);

        Task<MedicalRecordDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(MedicalRecordDto dto);

        Task<bool> UpdateAsync(int id, MedicalRecordDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
