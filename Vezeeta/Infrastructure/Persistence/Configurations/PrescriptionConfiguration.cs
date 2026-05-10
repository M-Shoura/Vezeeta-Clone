using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            // Table Name
            builder.ToTable("Prescriptions");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.PrescriptionDate)
                   .IsRequired();

            builder.Property(p => p.Notes)
                   .HasMaxLength(2000);

            // Relationships

            // Prescription -> Appointment
            builder.HasOne(p => p.Appointment)
                   .WithOne(a => a.Prescription)
                   .HasForeignKey<Prescription>(p => p.AppointmentId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Optional index
            builder.HasIndex(p => p.AppointmentId);
        }
    }
}