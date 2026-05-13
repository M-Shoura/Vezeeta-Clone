using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
    {
        public void Configure(EntityTypeBuilder<PatientProfile> builder)
        {
            // Table
            builder.ToTable("PatientProfiles");

            // Primary Key
            builder.HasKey(p => p.ApplicationUserId);

            // One-to-One with ApplicationUser
            builder.HasOne(p => p.ApplicationUser)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(
                    p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Properties

            builder.Property(p => p.DateOfBirth)
                   .IsRequired();

            builder.Property(p => p.Gender)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(p => p.BloodType)
                   .HasMaxLength(10);

            builder.Property(p => p.EmergencyContactName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.EmergencyContactPhone)
                   .IsRequired()
                   .HasMaxLength(20);

            // Ignore computed property
            builder.Ignore(p => p.Age);

            // Relationships

            // Patient -> Appointments
            builder.HasMany(p => p.Appointments)
                   .WithOne(a => a.Patient)
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Patient -> MedicalRecords
            builder.HasMany(p => p.MedicalRecords)
                   .WithOne(m => m.Patient)
                   .HasForeignKey(m => m.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Patient -> Reviews
            builder.HasMany(p => p.Reviews)
                   .WithOne(r => r.Patient)
                   .HasForeignKey(r => r.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}