using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(o => o.Name).HasMaxLength(200);
        builder.Property(o => o.Code).HasMaxLength(50);
        builder.Property(o => o.Description).HasMaxLength(4000);
        builder.Property(o => o.Website).HasMaxLength(500);
        builder.Property(o => o.ContactEmail).HasMaxLength(256);
        builder.Property(o => o.Address).HasMaxLength(500);
        builder.Property(o => o.TimeZoneId).HasMaxLength(100);
        builder.Property(o => o.DefaultCurrency).HasMaxLength(10);
        builder.HasIndex(o => o.Code).IsUnique();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(40);
    }
}
