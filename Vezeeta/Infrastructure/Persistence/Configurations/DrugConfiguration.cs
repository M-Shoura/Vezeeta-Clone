using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infranstructure.Persistence.Configurations
{
    public class DrugConfiguration : IEntityTypeConfiguration<Drug>
    {
        public void Configure(EntityTypeBuilder<Drug> builder)
        {
            // Table Name
            builder.ToTable("Drugs");

            // Primary Key
            builder.HasKey(d => d.Id);

            // Properties
            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(d => d.Description)
                   .HasMaxLength(1000);

            builder.Property(d => d.Price)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(d => d.GenericName)
                   .HasMaxLength(200);

            builder.Property(d => d.Manufacturer)
                   .HasMaxLength(200);

            builder.Property(d => d.Strength)
                   .HasMaxLength(100);

            builder.Property(d => d.SideEffects)
                   .HasMaxLength(2000);

            // Indexes
            builder.HasIndex(d => d.Name);

            // Relationships
            builder.HasMany(d => d.PrescriptionItems)
                   .WithOne(pi => pi.Drug)
                   .HasForeignKey(pi => pi.DrugId);
        }
    }
}