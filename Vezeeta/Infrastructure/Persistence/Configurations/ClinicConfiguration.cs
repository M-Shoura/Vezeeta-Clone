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

            // Clinic -> Schedules
            builder.HasMany(c => c.Schedules)
                   .WithOne(s => s.Clinic)
                   .HasForeignKey(s => s.ClinicId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Clinic -> Appointments
            builder.HasMany(c => c.Appointments)
                   .WithOne(a => a.Clinic)
                   .HasForeignKey(a => a.ClinicId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Clinic -> DoctorClinics
            builder.HasMany(c => c.DoctorClinics)
                   .WithOne(dc => dc.Clinic)
                   .HasForeignKey(dc => dc.ClinicId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}