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
            builder.Property(a => a.AppointmentDate).IsRequired();
            builder.Property(a => a.StartTime).IsRequired();
            builder.Property(a => a.EndTime).IsRequired();
            builder.Property(a => a.Status).IsRequired();
            builder.Property(a => a.Notes).HasMaxLength(1000);

            // Relationships
            builder.HasOne(a=> a.Doctor)
                   .WithMany(d => d.Appointments)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Patient)
                   .WithMany(p => p.Appointments)
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Clinic)
                   .WithMany(c => c.Appointments)
                   .HasForeignKey(a => a.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            // NOTE: Payment, Review, and Prescription are ONE-TO-ONE relationships.
            // These should be configured ONLY in their dependent entity configurations
            // (PaymentConfiguration, ReviewConfiguration, PrescriptionConfiguration)
            // to avoid duplicate/conflicting relationship mappings.
            // See Fluent API Best Practice: Configure relationships on the DEPENDENT entity (the one with the FK).

            // builder.HasOne(a => a.Payment)
            //      .WithOne(p => p.Appointment)
            //      .HasForeignKey<Payment>(p => p.AppointmentId)
            //      .OnDelete(DeleteBehavior.SetNull);

            // builder.HasOne(a => a.Review)
            //        .WithOne(r => r.Appointment)
            //        .HasForeignKey<Review>(r => r.AppointmentId)
            //        .OnDelete(DeleteBehavior.Restrict);

            // builder.HasOne(a => a.Prescription)
            //        .WithOne(p => p.Appointment)
            //        .HasForeignKey<Prescription>(p => p.AppointmentId)
            //        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
