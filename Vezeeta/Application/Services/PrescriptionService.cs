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
                .FindAllAsync(p => true, includes: new[]
                {
                    "Appointment",
                    "PrescriptionItems",
                    "PrescriptionItems.Drug"
                });

            return prescriptions.Select(MapPrescription);
        }

        public async Task<PrescriptionDto?> GetByIdAsync(int id)
        {
            var prescription = await _unitOfWork
                .Repository<Prescription>()
                .FindAsync(p => p.Id == id, includes: new[]
                {
                    "Appointment",
                    "PrescriptionItems",
                    "PrescriptionItems.Drug"
                });

            if (prescription == null)
                return null;

            return MapPrescription(prescription);
        }

        public async Task<PrescriptionDto?> GetByAppointmentIdAsync(int appointmentId)
        {
            var prescription = await _unitOfWork
                .Repository<Prescription>()
                .FindAsync(p => p.AppointmentId == appointmentId, includes: new[]
                {
                    "Appointment",
                    "PrescriptionItems",
                    "PrescriptionItems.Drug"
                });

            if (prescription == null)
                return null;

            return MapPrescription(prescription);
        }

        public async Task<bool> CreateAsync(PrescriptionDto dto)
        {
            if (dto.AppointmentId == null || dto.AppointmentId <= 0)
                return false;

            var existingPrescription = await _unitOfWork
                .Repository<Prescription>()
                .FindAsync(p => p.AppointmentId == dto.AppointmentId.Value);

            if (existingPrescription != null)
                return false;

            var prescription = new Prescription
            {
                AppointmentId = dto.AppointmentId.Value,
                Notes = dto.Notes,
                PrescriptionDate = dto.PrescriptionDate == default
                    ? DateTime.UtcNow
                    : dto.PrescriptionDate
            };

            await _unitOfWork
                .Repository<Prescription>()
                .AddAsync(prescription);

            await _unitOfWork.SaveChangesAsync();

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var item in dto.Items)
                {
                    item.PrescriptionId = prescription.Id;

                    if (item.DrugId <= 0)
                        continue;

                    var prescriptionItem = new PrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        DrugId = item.DrugId,
                        Dosage = item.Dosage,
                        DurationInDays = item.DurationInDays,
                        TimesPerDay = item.TimesPerDay,
                        Instructions = item.Instructions
                    };

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

                    if (item.DrugId <= 0)
                        continue;

                    var mappedItem = new PrescriptionItem
                    {
                        PrescriptionId = id,
                        DrugId = item.DrugId,
                        Dosage = item.Dosage,
                        DurationInDays = item.DurationInDays,
                        TimesPerDay = item.TimesPerDay,
                        Instructions = item.Instructions
                    };

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

        private static PrescriptionDto MapPrescription(Prescription prescription)
        {
            return new PrescriptionDto
            {
                Id = prescription.Id,
                AppointmentId = prescription.AppointmentId,
                PrescriptionDate = prescription.PrescriptionDate,
                Notes = prescription.Notes,
                AppointmentInfo = prescription.Appointment == null
                    ? null
                    : $"{prescription.Appointment.AppointmentDate:dd MMM yyyy} {prescription.Appointment.StartTime:hh\\:mm}",
                Items = prescription.PrescriptionItems.Select(item => new PrescriptionItemDto
                {
                    Id = item.Id,
                    PrescriptionId = item.PrescriptionId,
                    DrugId = item.DrugId ?? 0,
                    DrugName = item.Drug?.Name,
                    Dosage = item.Dosage,
                    DurationInDays = item.DurationInDays,
                    TimesPerDay = item.TimesPerDay,
                    Instructions = item.Instructions
                }).ToList()
            };
        }
    }
}
