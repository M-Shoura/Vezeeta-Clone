using Application.DTOs.Doctors;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        #region Doctor Queries

        public async Task<IEnumerable<DoctorDto>>
            GetAllDoctorsAsync()
        {
            return await _doctorRepository
                .GetAllDoctorsAsync();
        }

        public async Task<DoctorDto?>
            GetDoctorByIdAsync(
                string doctorId)
        {
            return await _doctorRepository
                .GetDoctorByIdAsync(doctorId);
        }

        public async Task<IEnumerable<DoctorDto>>
            GetAvailableDoctorsAsync()
        {
            return await _doctorRepository
                .GetAvailableDoctorsAsync();
        }

        public async Task<IEnumerable<DoctorDto>>
            GetDoctorsBySpecializationAsync(
                string specialization)
        {
            return await _doctorRepository
                .GetDoctorsBySpecializationAsync(
                    specialization);
        }

        #endregion

        #region Doctor CRUD

        public async Task CreateDoctorAsync(
            DoctorProfile doctor)
        {
            await _unitOfWork
                .Repository<DoctorProfile>()
                .AddAsync(doctor);

            await _unitOfWork
                .SaveChangesAsync();
        }

        public async Task UpdateDoctorAsync(
            DoctorProfile doctor)
        {
            var existingDoctor = await _unitOfWork
                .Repository<DoctorProfile>()
                .FindAsync(
                    d => d.ApplicationUserId
                        == doctor.ApplicationUserId);

            if (existingDoctor == null)
                throw new Exception(
                    "Doctor not found");

            existingDoctor.Specialization
                = doctor.Specialization;

            existingDoctor.YearsOfExperience
                = doctor.YearsOfExperience;

            existingDoctor.Bio
                = doctor.Bio;

            existingDoctor.Qualification
                = doctor.Qualification;

            existingDoctor.IsAvailable
                = doctor.IsAvailable;

            existingDoctor.LicenseNumber
                = doctor.LicenseNumber;

            _unitOfWork
                .Repository<DoctorProfile>()
                .Update(existingDoctor);

            await _unitOfWork
                .SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(
            string doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<DoctorProfile>()
                .FindAsync(
                    d => d.ApplicationUserId
                        == doctorId);

            if (doctor == null)
                throw new Exception(
                    "Doctor not found");

            _unitOfWork
                .Repository<DoctorProfile>()
                .Delete(doctor);

            await _unitOfWork
                .SaveChangesAsync();
        }

        #endregion

        #region Availability

        public async Task UpdateDoctorAvailabilityAsync(
            string doctorId,
            bool isAvailable)
        {
            var doctor = await _unitOfWork
                .Repository<DoctorProfile>()
                .FindAsync(
                    d => d.ApplicationUserId
                        == doctorId);

            if (doctor == null)
                throw new Exception(
                    "Doctor not found");

            doctor.IsAvailable = isAvailable;

            _unitOfWork
                .Repository<DoctorProfile>()
                .Update(doctor);

            await _unitOfWork
                .SaveChangesAsync();
        }

        #endregion

        #region Doctor Clinics

        public async Task AssignDoctorToClinicAsync(
            string doctorId,
            int clinicId,
            decimal fee)
        {
            var doctorClinic =
                new DoctorClinic
                {
                    DoctorId = doctorId,
                    ClinicId = clinicId,
                    ConsultationFee = fee
                };

            await _unitOfWork
                .Repository<DoctorClinic>()
                .AddAsync(doctorClinic);

            await _unitOfWork
                .SaveChangesAsync();
        }

        #endregion

        #region Doctor Schedules

        public async Task CreateDoctorScheduleAsync(
            DoctorSchedule schedule)
        {
            if (schedule.StartTime
                >= schedule.EndTime)
            {
                throw new Exception(
                    "Start time must be before end time");
            }

            var existingSchedules =
                await _unitOfWork
                    .Repository<DoctorSchedule>()
                    .FindAllAsync(
                        s =>
                            s.DoctorId
                                == schedule.DoctorId
                            &&
                            s.Day
                                == schedule.Day);

            bool hasConflict =
                existingSchedules.Any(
                    s =>
                        schedule.StartTime
                            < s.EndTime
                        &&
                        schedule.EndTime
                            > s.StartTime);

            if (hasConflict)
            {
                throw new Exception(
                    "Schedule overlaps with existing schedule");
            }

            await _unitOfWork
                .Repository<DoctorSchedule>()
                .AddAsync(schedule);

            await _unitOfWork
                .SaveChangesAsync();
        }

        public async Task<IEnumerable<DoctorSchedule>>
            GetDoctorSchedulesAsync(
                string doctorId)
        {
            return await _unitOfWork
                .Repository<DoctorSchedule>()
                .FindAllAsync(
                    s => s.DoctorId == doctorId,
                    includes: new[]
                    {
                        "Clinic"
                    });
        }

        public async Task<DoctorSchedule?>
            GetScheduleByIdAsync(
                int scheduleId)
        {
            return await _unitOfWork
                .Repository<DoctorSchedule>()
                .FindAsync(
                    s => s.Id == scheduleId,
                    includes: new[]
                    {
                        "Clinic",
                        "Doctor"
                    });
        }

        public async Task<string>
            DeleteScheduleAsync(
                int scheduleId)
        {
            var schedule = await _unitOfWork
                .Repository<DoctorSchedule>()
                .FindAsync(
                    s => s.Id == scheduleId);

            if (schedule == null)
            {
                throw new Exception(
                    "Schedule not found");
            }

            string doctorId =
                schedule.DoctorId;

            _unitOfWork
                .Repository<DoctorSchedule>()
                .Delete(schedule);

            await _unitOfWork
                .SaveChangesAsync();

            return doctorId;
        }

        #endregion
    }
}