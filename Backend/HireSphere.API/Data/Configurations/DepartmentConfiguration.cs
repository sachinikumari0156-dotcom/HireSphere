using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(200);
        builder.Property(d => d.Code).HasMaxLength(50);
        builder.Property(d => d.Description).HasMaxLength(4000);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(d => new { d.OrganizationId, d.Name }).IsUnique();

        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Departments)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ManagerUser)
            .WithMany()
            .HasForeignKey(d => d.ManagerUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
