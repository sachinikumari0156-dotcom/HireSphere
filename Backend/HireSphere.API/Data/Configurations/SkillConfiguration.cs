using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasIndex(s => s.Name).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(200);
        builder.Property(s => s.Description).HasMaxLength(4000);
        builder.Property(s => s.Category).HasMaxLength(100);
    }
}
