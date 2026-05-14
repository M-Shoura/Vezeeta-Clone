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

        public PatientProfileService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public Task ChangePasswordAsync(ChangePasswordDto model)
        {
            throw new NotImplementedException();
        }

        public Task<PatientAppointmentsDto> GetAllCurrentPatientAppointmentsAsync()
        {
            throw new NotImplementedException();
        }

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
                FullName = patientProfile.ApplicationUser.FullName,
                Email = patientProfile.ApplicationUser.Email,
                PhoneNumber = patientProfile.ApplicationUser.PhoneNumber,
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

            patientProfile.ApplicationUser.FullName = model.FullName;
            patientProfile.ApplicationUser.Email = model.Email;
            patientProfile.ApplicationUser.PhoneNumber = model.PhoneNumber;
            patientProfile.DateOfBirth = model.BirthDate;

            _unitOfWork
                .Repository<PatientProfile>()
                .Update(patientProfile);

            return _unitOfWork
                .SaveChangesAsync();
        }
    }
}