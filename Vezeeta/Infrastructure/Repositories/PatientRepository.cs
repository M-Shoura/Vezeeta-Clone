using Application.DTOs.Profiles.Patients;
using Application.Interfaces.Repositories;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientDto>>
            GetAllPatientsAsync()
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Select(p => new PatientDto
                {
                    ApplicationUserId = p.ApplicationUserId,
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    ProfilePicture = p.ApplicationUser.ProfilePicture,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    BloodType = p.BloodType,
                    EmergencyContactName = p.EmergencyContactName ?? string.Empty,
                    EmergencyContactPhone = p.EmergencyContactPhone ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<PatientDto?>
            GetPatientByIdAsync(string patientId)
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Where(p =>
                    p.ApplicationUserId == patientId)
                .Select(p => new PatientDto
                {
                    ApplicationUserId = p.ApplicationUserId,
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    ProfilePicture = p.ApplicationUser.ProfilePicture,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    BloodType = p.BloodType,
                    EmergencyContactName = p.EmergencyContactName ?? string.Empty,
                    EmergencyContactPhone = p.EmergencyContactPhone ?? string.Empty
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatientDto>>
            GetAvailablePatientsAsync()
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Select(p => new PatientDto
                {
                    ApplicationUserId = p.ApplicationUserId,
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    ProfilePicture = p.ApplicationUser.ProfilePicture,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    BloodType = p.BloodType,
                    EmergencyContactName = p.EmergencyContactName ?? string.Empty,
                    EmergencyContactPhone = p.EmergencyContactPhone ?? string.Empty
                })
                .ToListAsync();
        }
    }
}
