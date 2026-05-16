using Application.DTOs.Appointments;
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


        public async Task<IEnumerable<AvailableSlotDto>>
    GetAvailableSlotsAsync(
        string doctorId,
        DateTime date)
        {
            var day = date.DayOfWeek;

            // doctor schedules
            var schedules = await _unitOfWork
                .Repository<DoctorSchedule>()
                .FindAllAsync(
                    s =>
                        s.DoctorId == doctorId
                        &&
                        s.Day == day
                        &&
                        s.IsActive,
                    includes: new[] { "Clinic" });

            // booked appointments
            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .FindAllAsync(
                    a =>
                        a.DoctorId == doctorId
                        &&
                        a.AppointmentDate.Date
                            == date.Date);

            var availableSlots =
                new List<AvailableSlotDto>();

            foreach (var schedule in schedules)
            {
                var current =
                    schedule.StartTime;

                while (current
                       < schedule.EndTime)
                {
                    var slotEnd =
                        current.Add(
                            TimeSpan.FromMinutes(
                                schedule
                                .SlotDurationMinutes));

                    bool isBooked =
                        appointments.Any(a =>
                            a.StartTime == current
                            &&
                            a.EndTime == slotEnd);

                    if (!isBooked)
                    {
                        availableSlots.Add(
                            new AvailableSlotDto
                            {
                                Date = date,
                                StartTime = current,
                                EndTime = slotEnd,
                                ClinicId = schedule.ClinicId,
                                ClinicName = schedule.Clinic?.Name
                            });
                    }

                    current = slotEnd;
                }
            }

            return availableSlots;
        }
        public DoctorService(
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorDetailsDto?>
    GetDoctorDetailsAsync(string doctorId)
        {
            var doctor = await _unitOfWork
                .Repository<DoctorProfile>()
                .FindAsync(
                    d => d.ApplicationUserId == doctorId,
                    includes: new[]
                    {
                "ApplicationUser",
                "Reviews",
                "DoctorSchedules",
                "DoctorSchedules.Clinic"
                    });

            if (doctor == null)
                return null;

            // Generate next available dates
            var availableDates = new List<DateTime>();

            foreach (var schedule in doctor.DoctorSchedules)
            {
                for (int i = 0; i < 14; i++)
                {
                    var date = DateTime.Today.AddDays(i);

                    if (date.DayOfWeek.ToString()
                        == schedule.Day.ToString())
                    {
                        availableDates.Add(
                            date.Date
                            + schedule.StartTime);
                    }
                }
            }

            return new DoctorDetailsDto
            {
                Id = doctor.ApplicationUserId,
                FullName = doctor.ApplicationUser.FullName,
                ProfilePicture =
                    doctor.ApplicationUser.ProfilePicture,
                Specialization = doctor.Specialization,
                YearsOfExperience =
                    doctor.YearsOfExperience,
                Bio = doctor.Bio,
                Qualification = doctor.Qualification,
                IsAvailable = doctor.IsAvailable,

                Reviews = doctor.Reviews,
                Schedules = doctor.DoctorSchedules,

                AvailableDates = availableDates
                    .OrderBy(d => d)
            };
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
            if (string.IsNullOrWhiteSpace(schedule.DoctorId))
                throw new Exception("DoctorId is required");

            if (schedule.ClinicId <= 0)
                throw new Exception("ClinicId must be provided");

            if (schedule.SlotDurationMinutes <= 0)
                throw new Exception("Slot duration must be greater than zero");

            if (schedule.StartTime >= schedule.EndTime)
            {
                throw new Exception(
                    "Start time must be before end time");
            }

            var doctorClinic = await _unitOfWork
                .Repository<DoctorClinic>()
                .FindAsync(
                    dc =>
                        dc.DoctorId == schedule.DoctorId
                        && dc.ClinicId == schedule.ClinicId
                        && dc.IsAvailable);

            if (doctorClinic == null)
                throw new Exception("Doctor is not assigned to this clinic");

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

        public async Task<IEnumerable<Appointment>>
            GetDoctorAppointmentsAsync(string doctorId)
        {
            return await _unitOfWork
                .Repository<Appointment>()
                .FindAllAsync(
                    a => a.DoctorId == doctorId,
                    includes: new[]
                    {
                        "Patient",
                        "Patient.ApplicationUser",
                        "Clinic"
                    });
        }

        public async Task<IEnumerable<DoctorClinic>>
            GetDoctorClinicsAsync(string doctorId)
        {
            return await _unitOfWork
                .Repository<DoctorClinic>()
                .FindAllAsync(
                    dc => dc.DoctorId == doctorId,
                    includes: new[] { "Clinic" });
        }

        public async Task<IEnumerable<Clinic>>
            GetClinicsForDoctorAsync(string doctorId)
        {
            var doctorClinics = await GetDoctorClinicsAsync(doctorId);

            return doctorClinics
                .Where(dc => dc.IsAvailable && dc.Clinic != null && dc.Clinic.IsActive)
                .Select(dc => dc.Clinic)
                .OrderBy(c => c.Name);
        }

        public async Task UpdateConsultationFeeAsync(
            string doctorId,
            int clinicId,
            decimal newFee)
        {
            var doctorClinic = await _unitOfWork
                .Repository<DoctorClinic>()
                .FindAsync(
                    dc => dc.DoctorId == doctorId
                          && dc.ClinicId == clinicId);

            if (doctorClinic == null)
                throw new Exception("Doctor-Clinic assignment not found");

            doctorClinic.ConsultationFee = newFee;

            _unitOfWork
                .Repository<DoctorClinic>()
                .Update(doctorClinic);

            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        public async Task<IEnumerable<DoctorDto>>
    SearchDoctorsAsync(
        string? name,
        string? specialization)
        {
            var doctors = await _unitOfWork
                .Repository<DoctorProfile>()
                .FindAllAsync(
                    d =>
                        (string.IsNullOrEmpty(name)
                            || d.ApplicationUser.FullName.Contains(name))
                        &&
                        (string.IsNullOrEmpty(specialization)
                            || d.Specialization.Contains(specialization)),
                    includes: new[]
                    {
                "ApplicationUser"
                    });

            return doctors.Select(d => new DoctorDto
            {
                Id = d.ApplicationUserId,
                FullName = d.ApplicationUser.FullName,
                ProfilePicture = d.ApplicationUser.ProfilePicture,
                Specialization = d.Specialization,
                YearsOfExperience = d.YearsOfExperience,
                IsAvailable = d.IsAvailable,
                Qualification = d.Qualification,
                Bio = d.Bio
            });
        }

        public async Task<IEnumerable<Clinic>>
            GetAllClinicsAsync()
        {
            return await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync();
        }

        public async Task<IEnumerable<string>>
            GetAllSpecializationsAsync()
        {
            var doctors = await _unitOfWork
                .Repository<DoctorProfile>()
                .GetAllAsync();

            return doctors
                .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
                .Select(d => d.Specialization)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s);
        }
    }
}
