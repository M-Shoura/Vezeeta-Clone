using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
    {
        public void Configure(EntityTypeBuilder<DoctorProfile> builder)
        {
            // Table Name
            builder.ToTable("DoctorProfiles");

            // Primary Key
            builder.HasKey(d => d.ApplicationUserId);

            // Properties

            builder.Property(d => d.Specialization)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(d => d.YearsOfExperience)
                   .IsRequired();

            builder.Property(d => d.Bio)
                   .HasMaxLength(2000);

            builder.Property(d => d.Qualification)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(d => d.IsAvailable)
                   .HasDefaultValue(true);

            builder.Property(d => d.LicenseNumber)
                   .HasMaxLength(100);

            // Indexes (important for search performance)
            builder.HasIndex(d => d.ApplicationUserId);
            builder.HasIndex(d => d.LicenseNumber)
                   .IsUnique()
                   .HasFilter("[LicenseNumber] IS NOT NULL");

            // Relationships

            // DoctorProfile -> DoctorClinic
            builder.HasMany(d => d.DoctorClinics)
                   .WithOne(dc => dc.Doctor)
                   .HasForeignKey(dc => dc.DoctorId);

            // DoctorProfile -> Appointment
            builder.HasMany(d => d.Appointments)
                   .WithOne(a => a.Doctor)
                   .HasForeignKey(a => a.DoctorId);

            // DoctorProfile -> DoctorSchedule
            builder.HasMany(d => d.DoctorSchedules)
                   .WithOne(s => s.Doctor)
                   .HasForeignKey(s => s.DoctorId);
                   
            // DoctorProfile -> Review
            builder.HasMany(d => d.Reviews)
                   .WithOne(r => r.Doctor)
                   .HasForeignKey(r => r.DoctorId);

            
        }
    }
}