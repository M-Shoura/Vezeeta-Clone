using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            // Table Name
            builder.ToTable("DoctorSchedules");

            // Primary Key
            builder.HasKey(ds => ds.Id);

            // Properties
            builder.Property(ds => ds.StartTime)
                .IsRequired();

            builder.Property(ds => ds.EndTime)
                .IsRequired();

            builder.Property(ds => ds.SlotDurationMinutes)
                .HasDefaultValue(30); // Default slot duration of 30 minutes

            builder.Property(ds => ds.IsActive)
                .HasDefaultValue(true);

            // Relationships

            // DoctorSchedule -> Doctor
            builder.HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSchedules) 
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // DoctorSchedule -> Clinic
            builder.HasOne(ds => ds.Clinic)
                .WithMany(c => c.DoctorSchedules) 
                .HasForeignKey(ds => ds.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
