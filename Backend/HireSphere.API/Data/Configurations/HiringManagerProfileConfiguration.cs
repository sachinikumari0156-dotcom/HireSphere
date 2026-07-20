using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class HiringManagerProfileConfiguration : IEntityTypeConfiguration<HiringManagerProfile>
{
    public void Configure(EntityTypeBuilder<HiringManagerProfile> builder)
    {
        builder.HasIndex(h => h.UserId).IsUnique();

        builder.Property(h => h.PhoneNumber).HasMaxLength(50);
        builder.Property(h => h.Title).HasMaxLength(200);

        builder.HasOne(h => h.Organization)
            .WithMany(o => o.HiringManagerProfiles)
            .HasForeignKey(h => h.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Department)
            .WithMany(d => d.HiringManagerProfiles)
            .HasForeignKey(h => h.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
