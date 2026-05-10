using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class DoctorClinicConfiguration : IEntityTypeConfiguration<DoctorClinic>
    {
        public void Configure(EntityTypeBuilder<DoctorClinic> builder)
        {

            // Table Name
            builder.ToTable("DoctorClinics");

            // Primary Key - Composite Key of DoctorId and ClinicId
            builder.HasKey(dc => new { dc.DoctorId, dc.ClinicId });

            // Properties
            builder.Property(dc => dc.ConsultationFee)
                .HasPrecision(18, 2) // Standard for Egyptian Pounds/Currency
                .IsRequired();

            builder.Property(dc => dc.IsAvailable)
                .HasDefaultValue(true);

            // Relationships

            // DoctorClinic -> Doctor
            builder.HasOne(dc => dc.Doctor)
                .WithMany(d => d.DoctorClinics) 
                .HasForeignKey(dc => dc.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // DoctorClinic -> Clinic
            builder.HasOne(dc => dc.Clinic)
                .WithMany(c => c.DoctorClinics)
                .HasForeignKey(dc => dc.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
