using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class RecruiterProfileConfiguration : IEntityTypeConfiguration<RecruiterProfile>
{
    public void Configure(EntityTypeBuilder<RecruiterProfile> builder)
    {
        builder.HasIndex(r => r.UserId).IsUnique();

        builder.Property(r => r.PhoneNumber).HasMaxLength(50);
        builder.Property(r => r.Title).HasMaxLength(200);

        builder.HasOne(r => r.Organization)
            .WithMany(o => o.RecruiterProfiles)
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Department)
            .WithMany(d => d.RecruiterProfiles)
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
