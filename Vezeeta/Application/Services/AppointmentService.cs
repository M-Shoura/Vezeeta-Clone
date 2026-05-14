using Vezeeta.Application.DTOs.Appointments;
using Vezeeta.Domain.Entities;
using Vezeeta.Domain.Enums;
using Vezeeta.Domain.Interfaces.Repositories;
using Vezeeta.Domain.Interfaces.Services;

namespace Vezeeta.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork)
        {
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAppointmentsAsync();
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            return await _appointmentRepository.GetAppointmentByIdAsync(id);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _appointmentRepository.GetAppointmentsByDoctorAsync(doctorId);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _appointmentRepository.GetAppointmentsByPatientAsync(patientId);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByClinicAsync(int clinicId)
        {
            return await _appointmentRepository.GetAppointmentsByClinicAsync(clinicId);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _appointmentRepository.GetAppointmentsByDateRangeAsync(startDate, endDate);
        }

        public async Task<int> CreateAppointmentAsync(CreateAppointmentDto createDto, string userId)
        {
            // Validate that the time is in the future
            var appointmentDateTime = createDto.AppointmentDate.Add(createDto.StartTime);
            if (appointmentDateTime <= DateTime.Now)
            {
                throw new InvalidOperationException("Appointment date and time must be in the future.");
            }

            // Validate start time is before end time
            if (createDto.StartTime >= createDto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            // Check if slot is available
            var isAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                createDto.DoctorId,
                createDto.ClinicId,
                createDto.AppointmentDate,
                createDto.StartTime,
                createDto.EndTime);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            var appointment = new Appointment
            {
                AppointmentDate = createDto.AppointmentDate,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                DoctorId = createDto.DoctorId.ToString(),
                PatientId = createDto.PatientId.ToString(),
                ClinicId = createDto.ClinicId,
                Notes = createDto.Notes,
                Status = AppointmentStatus.Pending,
                CreatedBy = userId,
                CreatedOn = DateTime.Now
            };

            await _appointmentRepository.AddAppointmentAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<bool> UpdateAppointmentAsync(UpdateAppointmentDto updateDto, string userId)
        {
            var appointment = await _appointmentRepository.GetAppointmentEntityByIdAsync(updateDto.Id);

            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }

            // Cannot update if appointment is completed or cancelled
            if (appointment.Status == AppointmentStatus.Completed || 
                appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot update a {appointment.Status} appointment.");
            }

            // Validate that the time is in the future
            var appointmentDateTime = updateDto.AppointmentDate.Add(updateDto.StartTime);
            if (appointmentDateTime <= DateTime.Now)
            {
                throw new InvalidOperationException("Appointment date and time must be in the future.");
            }

            // Validate start time is before end time
            if (updateDto.StartTime >= updateDto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            // Check if new slot is available (excluding current appointment)
            var isAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                int.Parse(appointment.DoctorId ?? "0"),
                updateDto.ClinicId,
                updateDto.AppointmentDate,
                updateDto.StartTime,
                updateDto.EndTime);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            appointment.AppointmentDate = updateDto.AppointmentDate;
            appointment.StartTime = updateDto.StartTime;
            appointment.EndTime = updateDto.EndTime;
            appointment.ClinicId = updateDto.ClinicId;
            appointment.Notes = updateDto.Notes;
            appointment.LastModifiedBy = userId;
            appointment.LastModifiedOn = DateTime.Now;

            await _appointmentRepository.UpdateAppointmentAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
        {
            var appointment = await _appointmentRepository.GetAppointmentEntityByIdAsync(appointmentId);

            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException("Appointment is already cancelled.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed appointment.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.LastModifiedBy = userId;
            appointment.LastModifiedOn = DateTime.Now;

            await _appointmentRepository.UpdateAppointmentAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(int appointmentId, string userId)
        {
            var appointment = await _appointmentRepository.GetAppointmentEntityByIdAsync(appointmentId);

            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Appointment is already completed.");
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot complete a cancelled appointment.");
            }

            appointment.Status = AppointmentStatus.Completed;
            appointment.LastModifiedBy = userId;
            appointment.LastModifiedOn = DateTime.Now;

            await _appointmentRepository.UpdateAppointmentAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<(DateTime date, List<TimeSpan> availableSlots)>> GetAvailableSlotsAsync(
            int doctorId,
            int clinicId,
            DateTime startDate,
            DateTime endDate,
            int slotDurationMinutes = 30)
        {
            var result = new List<(DateTime, List<TimeSpan>)>();
            var businessHoursStart = new TimeSpan(9, 0, 0); // 9 AM
            var businessHoursEnd = new TimeSpan(17, 0, 0);   // 5 PM

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                // Skip past dates
                if (date < DateTime.Today)
                {
                    continue;
                }

                var availableSlots = new List<TimeSpan>();
                var currentTime = businessHoursStart;

                while (currentTime < businessHoursEnd)
                {
                    var slotEnd = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));

                    if (slotEnd > businessHoursEnd)
                    {
                        break;
                    }

                    var isAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                        doctorId,
                        clinicId,
                        date,
                        currentTime,
                        slotEnd);

                    if (isAvailable)
                    {
                        availableSlots.Add(currentTime);
                    }

                    currentTime = slotEnd;
                }

                if (availableSlots.Any())
                {
                    result.Add((date, availableSlots));
                }
            }

            return result;
        }

        public async Task<bool> IsSlotAvailableAsync(
            int doctorId,
            int clinicId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            return await _appointmentRepository.IsSlotAvailableAsync(
                doctorId,
                clinicId,
                appointmentDate,
                startTime,
                endTime);
        }
    }
}
