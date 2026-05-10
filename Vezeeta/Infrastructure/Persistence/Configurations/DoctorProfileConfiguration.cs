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
            // NOTE: This relationship is configured in DoctorClinicConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.
            // See Fluent API Best Practice: Configure relationships on the DEPENDENT entity (the one with the FK).

            // builder.HasMany(d => d.DoctorClinics)
            //        .WithOne(dc => dc.Doctor)
            //        .HasForeignKey(dc => dc.DoctorId)
            //        .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Appointments (one-to-many)
            // NOTE: This relationship is configured in AppointmentConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(d => d.Appointments)
            //        .WithOne(a => a.Doctor)
            //        .HasForeignKey(a => a.DoctorId)
            //        .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Schedules (one-to-many)
            // NOTE: This relationship is configured in DoctorScheduleConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(d => d.Schedules)
            //        .WithOne(s => s.Doctor)
            //        .HasForeignKey(s => s.DoctorId)
            //        .OnDelete(DeleteBehavior.SetNull);

            // DoctorProfile -> Reviews (one-to-many)
            // NOTE: Reviews are linked to Appointment, not directly to DoctorProfile.
            // This is already handled in ReviewConfiguration and AppointmentConfiguration.

            // builder.HasMany(d => d.Reviews)
            //        .WithOne(r => r.Doctor)
            //        .HasForeignKey(r => r.DoctorId)
            //        .OnDelete(DeleteBehavior.SetNull);
        }
    }
}