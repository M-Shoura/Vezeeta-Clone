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
            // Prescription is the DEPENDENT entity (has FK: AppointmentId)
            // So the relationship is configured HERE as the dependent endpoint.

            builder.HasOne(p => p.Appointment)
                   .WithOne(a => a.Prescription)
                   .HasForeignKey<Prescription>(p => p.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Prescription -> PrescriptionItems (one-to-many)
            // NOTE: This relationship is configured in PrescriptionItemConfiguration as the dependent entity.
            // Commented out here to avoid duplicate/conflicting relationship mappings.

            // builder.HasMany(p => p.Items)
            //        .WithOne(i => i.Prescription)
            //        .HasForeignKey(i => i.PrescriptionId)
            //        .OnDelete(DeleteBehavior.SetNull);

            // Optional index
            builder.HasIndex(p => p.AppointmentId);
        }
    }
}