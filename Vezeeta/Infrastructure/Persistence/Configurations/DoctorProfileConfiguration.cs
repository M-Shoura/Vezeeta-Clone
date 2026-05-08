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
            builder.HasKey(d => d.Id);

            // Properties
            builder.Property(d => d.ApplicationUserId)
                   .IsRequired();

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

            builder.Property(d => d.ProfileImage)
                   .HasMaxLength(500);

            builder.Property(d => d.LicenseNumber)
                   .HasMaxLength(100);

            // Indexes (important for search performance)
            builder.HasIndex(d => d.ApplicationUserId);
            builder.HasIndex(d => d.LicenseNumber)
                   .IsUnique()
                   .HasFilter("[LicenseNumber] IS NOT NULL");

            // Relationships

            // DoctorProfile -> DoctorClinics (many-to-many via join table)
            builder.HasMany(d => d.DoctorClinics)
                   .WithOne(dc => dc.Doctor)
                   .HasForeignKey(dc => dc.DoctorId)
                   .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Appointments
            builder.HasMany(d => d.Appointments)
                   .WithOne(a => a.Doctor)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Schedules
            builder.HasMany(d => d.Schedules)
                   .WithOne(s => s.Doctor)
                   .HasForeignKey(s => s.DoctorId)
                   .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Reviews
       //      builder.HasMany(d => d.Reviews)
       //             .WithOne(r => r.Doctor)
       //             .HasForeignKey(r => r.DoctorId)
       //             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}