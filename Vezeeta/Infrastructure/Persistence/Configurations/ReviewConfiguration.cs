using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Table Name
            builder.ToTable("Reviews");

            // Primary Key
            builder.HasKey(r => r.Id);

            // Properties
            builder.Property(r => r.Rating)
                   .IsRequired();

            builder.Property(r => r.Comment)
                   .HasMaxLength(2000);

            // Optional validation constraint (recommended)
            builder.Property(r => r.Rating)
                   .HasConversion<int>();

            // Relationships

            // Review -> Appointment
            builder.HasOne(r => r.Appointment)
                   .WithOne(a => a.Review)
                   .HasForeignKey<Review>(r => r.AppointmentId);

            // Review -> Patient
            builder.HasOne(r => r.Patient)
                   .WithMany(p => p.Reviews)
                   .HasForeignKey(r => r.PatientId);
                   
            // Review -> Doctor
            builder.HasOne(r => r.Doctor)
                   .WithMany(d => d.Reviews)
                   .HasForeignKey(r => r.DoctorId);

            // Indexes
            builder.HasIndex(r => r.AppointmentId);

            // Optional: enforce rating range (1–5) at DB level
            builder.ToTable(t =>
                t.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));
        }
    }
}