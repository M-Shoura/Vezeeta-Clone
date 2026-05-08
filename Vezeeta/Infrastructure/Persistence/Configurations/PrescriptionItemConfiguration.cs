using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            // Table Name
            builder.ToTable("PrescriptionItems");

            // Primary Key
            builder.HasKey(pi => pi.Id);

            // Properties
            builder.Property(x => x.Dosage)
            .HasMaxLength(100).IsRequired();

            builder.Property(x => x.Instructions)
            .HasMaxLength(1000);
            
            builder.Property(pi => pi.DurationInDays)
                .IsRequired();

            builder.Property(pi => pi.TimesPerDay)
                .IsRequired();


            // Relationships
            builder.HasOne(x => x.Prescription)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Drug)
                .WithMany(x => x.PrescriptionItems)
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
