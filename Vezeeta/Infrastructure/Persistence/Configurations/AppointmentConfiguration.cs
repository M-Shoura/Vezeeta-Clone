using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            // Table Name
            builder.ToTable("Appointments");

            // Primary Key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.AppointmentDate)
                    .IsRequired();

            builder.Property(a => a.StartTime)
                    .IsRequired();

            builder.Property(a => a.EndTime)
                    .IsRequired();

            builder.Property(a => a.Status)
                    .IsRequired();

            builder.Property(a => a.Notes).
                    HasMaxLength(1000);

            // Relationships
            builder.HasOne(a => a.Doctor)
                   .WithMany(d => d.Appointments)
                   .HasForeignKey(a => a.DoctorId);

            builder.HasOne(a => a.Patient)
                   .WithMany(p => p.Appointments)
                   .HasForeignKey(a => a.PatientId);

            builder.HasOne(a => a.Clinic)
                   .WithMany(c => c.Appointments)
                   .HasForeignKey(a => a.ClinicId);
        }
    }
}
