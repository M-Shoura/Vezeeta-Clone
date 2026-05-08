using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    internal class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            // Table Name
            builder.ToTable("MedicalRecords");

            // Primary Key
            builder.HasKey(mr => mr.Id);

            // Properties
            builder.Property(mr => mr.Condition)
                .HasMaxLength(250)
                .IsRequired();

            // Use Max length or large values for detailed text fields
            builder.Property(mr => mr.Description)
                .HasMaxLength(2000);

            builder.Property(mr => mr.DiagnosisCode)
                .HasMaxLength(50);

            builder.Property(mr => mr.Treatment)
                .HasMaxLength(2000);

            builder.Property(mr => mr.Allergies)
                .HasMaxLength(1000);

            builder.Property(mr => mr.Medications)
                .HasMaxLength(1000);

            builder.Property(mr => mr.IsActive)
                .HasDefaultValue(true);

            builder.Property(mr => mr.DiagnosedDate)
                .IsRequired();


            // Relationships
            builder.HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalRecords) 
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
