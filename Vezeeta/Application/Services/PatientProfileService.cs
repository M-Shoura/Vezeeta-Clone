using Application.DTOs.Profiles.Patients;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Domain.Identity;
using Application.DTOs.Profiles;
using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Services
{
    public class PatientProfileService : IPatientProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPatientRepository _patientRepository;

        public PatientProfileService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            IPatientRepository patientRepository)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _patientRepository = patientRepository;
        }

        #region Patient Queries

        public async Task<IEnumerable<PatientProfileDto>>
            GetAllPatientsAsync()
        {
            return await _patientRepository
                .GetAllPatientsAsync();
        }

        public async Task<PatientProfileDto?>
            GetPatientByIdAsync(string patientId)
        {
            return await _patientRepository
                .GetPatientByIdAsync(patientId);
        }

        #endregion

        #region Patient CRUD

        public async Task CreatePatientAsync(
            PatientProfile patient)
        {
            await _unitOfWork
                .Repository<PatientProfile>()
                .AddAsync(patient);

            await _unitOfWork
                .SaveChangesAsync();
        }

        public async Task UpdatePatientAsync(
            PatientProfile patient)
        {
            var existingPatient = await _unitOfWork
                .Repository<PatientProfile>()
                .FindAsync(
                    p => p.ApplicationUserId
                        == patient.ApplicationUserId);

            if (existingPatient == null)
                throw new Exception(
                    "Patient not found");

            existingPatient.DateOfBirth
                = patient.DateOfBirth;

            existingPatient.Gender
                = patient.Gender;

            existingPatient.BloodType
                = patient.BloodType;

            existingPatient.EmergencyContactName
                = patient.EmergencyContactName;

            existingPatient.EmergencyContactPhone
                = patient.EmergencyContactPhone;

            _unitOfWork
                .Repository<PatientProfile>()
                .Update(existingPatient);

            await _unitOfWork
                .SaveChangesAsync();
        }

        public async Task DeletePatientAsync(
            string patientId)
        {
            var patient = await _unitOfWork
                .Repository<PatientProfile>()
                .FindAsync(
                    p => p.ApplicationUserId
                        == patientId);

            if (patient == null)
                throw new Exception(
                    "Patient not found");

            _unitOfWork
                .Repository<PatientProfile>()
                .Delete(patient);

            await _unitOfWork
                .SaveChangesAsync();
        }

        #endregion

        #region Patient Profile

        public Task<PatientProfileDto> GetCurrentPatientProfileAsync()
        {
            var userId = _currentUser.UserId;

            var patientProfile = _unitOfWork
                .Repository<PatientProfile>()
                .Find(p => p.ApplicationUserId == userId, new string[] { "ApplicationUser" });

            if (patientProfile == null)
            {
                return Task.FromResult(new PatientProfileDto
                {
                    FullName = "John Doe",
                    Email = "OgKlC@example.com",
                    PhoneNumber = "1234567890",
                    BirthDate = new DateTime(2000, 1, 1)
                });
            }

            return Task.FromResult(new PatientProfileDto
            {
                FullName = patientProfile.ApplicationUser.FullName ?? string.Empty,
                Email = patientProfile.ApplicationUser.Email ?? string.Empty,
                PhoneNumber = patientProfile.ApplicationUser.PhoneNumber ?? string.Empty,
                BirthDate = patientProfile.DateOfBirth
            });
        }

        public Task<int> UpdatePatientProfileAsync(PatientProfileDto model)
        {
            var userId = _currentUser.UserId;

            var patientProfile = _unitOfWork
                .Repository<PatientProfile>()
                .Find(p => p.ApplicationUserId == userId);

            if (patientProfile == null)
                throw new Exception("Patient profile not found");

            patientProfile.ApplicationUser.FullName = model.FullName ?? string.Empty;
            patientProfile.ApplicationUser.Email = model.Email ?? string.Empty;
            patientProfile.ApplicationUser.PhoneNumber = model.PhoneNumber ?? string.Empty;
            patientProfile.DateOfBirth = model.BirthDate;

            _unitOfWork
                .Repository<PatientProfile>()
                .Update(patientProfile);

            return _unitOfWork
                .SaveChangesAsync();
        }

        public Task ChangePasswordAsync(ChangePasswordDto model)
        {
            throw new NotImplementedException();
        }

        public Task<PatientAppointmentsDto> GetAllCurrentPatientAppointmentsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}