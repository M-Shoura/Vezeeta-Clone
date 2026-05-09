using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            // Table Name
            builder.ToTable("Clinics");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Address)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.Email)
                   .HasMaxLength(255);

            builder.Property(c => c.Description)
                   .HasMaxLength(1000);

            builder.Property(c => c.IsActive)
                   .HasDefaultValue(true);

            // Relationships

            // Clinic -> Doctors
            // NOTE: This relationship is handled via DoctorClinic join table.
            // See DoctorClinicConfiguration for the join table relationships.
            // builder.HasMany(c => c.Doctors)
            //        .WithOne(d => d.Clinic)
            //        .HasForeignKey(d => d.ClinicId)
            //        .OnDelete(DeleteBehavior.Restrict);

            // Clinic -> DoctorSchedules (one-to-many)
            // NOTE: This relationship is configured in DoctorScheduleConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(c => c.Schedules)
            //        .WithOne(s => s.Clinic)
            //        .HasForeignKey(s => s.ClinicId)
            //        .OnDelete(DeleteBehavior.Cascade);

            // Clinic -> Appointments (one-to-many)
            // NOTE: This relationship is configured in AppointmentConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(c => c.Appointments)
            //        .WithOne(a => a.Clinic)
            //        .HasForeignKey(a => a.ClinicId)
            //        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}