using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Table Name
            builder.ToTable("Payments");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)").IsRequired();

            // Relationships

            // Payment -> Appointment
            builder.HasOne(p => p.Appointment)
                   .WithOne(a => a.Payment)
                   .HasForeignKey<Payment>(p => p.AppointmentId)
                   .OnDelete(DeleteBehavior.SetNull);
            
            // Index
            //builder.HasIndex(p => p.TransactionReference)
            //    .IsUnique()
            //    .HasFilter("[TransactionReference] IS NOT NULL");
        }
    }
}
