using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.Property(w => w.CompanyName).HasMaxLength(200);
        builder.Property(w => w.JobTitle).HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(4000);
        builder.Property(w => w.Location).HasMaxLength(200);
    }
}
