using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class SkillAssessmentConfiguration : IEntityTypeConfiguration<SkillAssessment>
{
    public void Configure(EntityTypeBuilder<SkillAssessment> builder)
    {
        builder.Property(sa => sa.Title).HasMaxLength(200);
        builder.Property(sa => sa.Description).HasMaxLength(4000);
        builder.Property(sa => sa.PassingScorePercent).HasPrecision(5, 2);

        builder.HasOne(sa => sa.Job)
            .WithMany()
            .HasForeignKey(sa => sa.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(sa => sa.Questions)
            .WithOne(q => q.SkillAssessment)
            .HasForeignKey(q => q.SkillAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sa => sa.Attempts)
            .WithOne(at => at.SkillAssessment)
            .HasForeignKey(at => at.SkillAssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sa => sa.Assignments)
            .WithOne(a => a.SkillAssessment)
            .HasForeignKey(a => a.SkillAssessmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
