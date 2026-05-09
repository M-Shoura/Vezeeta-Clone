using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
    {
        public void Configure(EntityTypeBuilder<PatientProfile> builder)
        {
            // Table Name
            builder.ToTable("PatientProfiles");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.ApplicationUserId)
                   .IsRequired();

            builder.Property(p => p.DateOfBirth)
                   .IsRequired();

            builder.Property(p => p.Gender)
                   .IsRequired();

            builder.Property(p => p.BloodType)
                   .HasMaxLength(10);

            builder.Property(p => p.EmergencyContactName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.EmergencyContactPhone)
                   .IsRequired()
                   .HasMaxLength(20);

            // Store enum as string (more readable in DB)
            builder.Property(p => p.Gender)
                   .HasConversion<string>();

            // Not mapped (computed property)
            builder.Ignore(p => p.Age);

            // Indexes
            builder.HasIndex(p => p.ApplicationUserId);

            // Relationships

            // PatientProfile -> Appointments (one-to-many)
            // NOTE: This relationship is configured in AppointmentConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.
            // See Fluent API Best Practice: Configure relationships on the DEPENDENT entity (the one with the FK).

            // builder.HasMany(p => p.Appointments)
            //        .WithOne(a => a.Patient)
            //        .HasForeignKey(a => a.PatientId)
            //        .OnDelete(DeleteBehavior.SetNull);

            // PatientProfile -> MedicalRecords (one-to-many)
            // NOTE: This relationship is configured in MedicalRecordConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(p => p.MedicalRecords)
            //        .WithOne(m => m.Patient)
            //        .HasForeignKey(m => m.PatientId)
            //        .OnDelete(DeleteBehavior.SetNull);
        }
    }
}