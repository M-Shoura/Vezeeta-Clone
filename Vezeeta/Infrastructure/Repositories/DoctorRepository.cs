using Application.DTOs.Doctors;
using Application.Interfaces.Repositories;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>>
            GetAllDoctorsAsync()
        {
            return await _context.DoctorProfiles
                .Include(d => d.ApplicationUser)
                .Select(d => new DoctorDto
                {
                    Id = d.ApplicationUserId,
                    FullName = d.ApplicationUser.FullName,
                    ProfilePicture =
                        d.ApplicationUser.ProfilePicture,
                    Email = d.ApplicationUser.Email,

                    Specialization = d.Specialization,
                    YearsOfExperience =
                        d.YearsOfExperience,

                    Bio = d.Bio,
                    Qualification = d.Qualification,
                    IsAvailable = d.IsAvailable,
                    LicenseNumber = d.LicenseNumber
                })
                .ToListAsync();
        }

        public async Task<DoctorDto?>
            GetDoctorByIdAsync(string doctorId)
        {
            return await _context.DoctorProfiles
                .Include(d => d.ApplicationUser)
                .Where(d =>
                    d.ApplicationUserId == doctorId)
                .Select(d => new DoctorDto
                {
                    Id = d.ApplicationUserId,
                    FullName = d.ApplicationUser.FullName,
                    ProfilePicture =
                        d.ApplicationUser.ProfilePicture,
                    Email = d.ApplicationUser.Email,

                    Specialization = d.Specialization,
                    YearsOfExperience =
                        d.YearsOfExperience,

                    Bio = d.Bio,
                    Qualification = d.Qualification,
                    IsAvailable = d.IsAvailable,
                    LicenseNumber = d.LicenseNumber
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DoctorDto>>
            GetAvailableDoctorsAsync()
        {
            return await _context.DoctorProfiles
                .Include(d => d.ApplicationUser)
                .Where(d => d.IsAvailable)
                .Select(d => new DoctorDto
                {
                    Id = d.ApplicationUserId,
                    FullName = d.ApplicationUser.FullName,
                    ProfilePicture =
                        d.ApplicationUser.ProfilePicture,
                    Email = d.ApplicationUser.Email,

                    Specialization = d.Specialization,
                    YearsOfExperience =
                        d.YearsOfExperience,

                    Bio = d.Bio,
                    Qualification = d.Qualification,
                    IsAvailable = d.IsAvailable,
                    LicenseNumber = d.LicenseNumber
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorDto>>
            GetDoctorsBySpecializationAsync(
                string specialization)
        {
            return await _context.DoctorProfiles
                .Include(d => d.ApplicationUser)
                .Where(d =>
                    d.Specialization == specialization)
                .Select(d => new DoctorDto
                {
                    Id = d.ApplicationUserId,
                    FullName = d.ApplicationUser.FullName,
                    ProfilePicture =
                        d.ApplicationUser.ProfilePicture,
                    Email = d.ApplicationUser.Email,

                    Specialization = d.Specialization,
                    YearsOfExperience =
                        d.YearsOfExperience,

                    Bio = d.Bio,
                    Qualification = d.Qualification,
                    IsAvailable = d.IsAvailable,
                    LicenseNumber = d.LicenseNumber
                })
                .ToListAsync();
        }
    }
}