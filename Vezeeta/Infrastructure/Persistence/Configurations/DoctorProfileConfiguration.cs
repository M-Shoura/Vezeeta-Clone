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

            // // DoctorProfile -> DoctorClinics (many-to-many via join table)
            // builder.HasMany(d => d.DoctorClinics)
            //        .WithOne(dc => dc.DoctorProfile)
            //        .HasForeignKey(dc => dc.DoctorProfileId)
            //        .OnDelete(DeleteBehavior.Cascade);

            // // DoctorProfile -> Appointments
            // builder.HasMany(d => d.Appointments)
            //        .WithOne(a => a.DoctorProfile)
            //        .HasForeignKey(a => a.DoctorProfileId)
            //        .OnDelete(DeleteBehavior.Restrict);

            // // DoctorProfile -> Schedules
            // builder.HasMany(d => d.Schedules)
            //        .WithOne(s => s.DoctorProfile)
            //        .HasForeignKey(s => s.DoctorProfileId)
            //        .OnDelete(DeleteBehavior.Cascade);

            // // DoctorProfile -> Reviews
            // builder.HasMany(d => d.Reviews)
            //        .WithOne(r => r.DoctorProfile)
            //        .HasForeignKey(r => r.DoctorProfileId)
            //        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}