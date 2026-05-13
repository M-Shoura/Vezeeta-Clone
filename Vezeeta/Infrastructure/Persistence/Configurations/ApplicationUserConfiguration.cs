using Domain.Entities;
using Domain.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName)
                            .IsRequired()
                            .HasMaxLength(200);

            builder.Property(u => u.Address)
                .HasMaxLength(300);

            builder.Property(u => u.ProfilePicture)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);
        }
    }
}
