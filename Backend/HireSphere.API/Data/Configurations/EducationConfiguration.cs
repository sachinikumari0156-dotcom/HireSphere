using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.Property(e => e.Institution).HasMaxLength(200);
        builder.Property(e => e.Degree).HasMaxLength(200);
        builder.Property(e => e.FieldOfStudy).HasMaxLength(200);
        builder.Property(e => e.Grade).HasMaxLength(50);
    }
}
