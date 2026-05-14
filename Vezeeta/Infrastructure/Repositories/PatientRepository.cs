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

        public async Task<IEnumerable<PatientProfileDto>>
            GetAllPatientsAsync()
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Select(p => new PatientProfileDto
                {
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth
                })
                .ToListAsync();
        }

        public async Task<PatientProfileDto?>
            GetPatientByIdAsync(string patientId)
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Where(p =>
                    p.ApplicationUserId == patientId)
                .Select(p => new PatientProfileDto
                {
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatientProfileDto>>
            GetAvailablePatientsAsync()
        {
            return await _context.PatientProfiles
                .Include(p => p.ApplicationUser)
                .Select(p => new PatientProfileDto
                {
                    FullName = p.ApplicationUser.FullName ?? string.Empty,
                    Email = p.ApplicationUser.Email ?? string.Empty,
                    PhoneNumber = p.ApplicationUser.PhoneNumber ?? string.Empty,
                    BirthDate = p.DateOfBirth
                })
                .ToListAsync();
        }
    }
}
