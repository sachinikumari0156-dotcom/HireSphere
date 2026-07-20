using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CandidateEvaluationConfiguration : IEntityTypeConfiguration<CandidateEvaluation>
{
    public void Configure(EntityTypeBuilder<CandidateEvaluation> builder)
    {
        builder.Property(e => e.OverallScore).HasPrecision(5, 2);
        builder.Property(e => e.RequiredSkillsAlignment).HasPrecision(5, 2);
        builder.Property(e => e.PreferredSkillsAlignment).HasPrecision(5, 2);
        builder.Property(e => e.RelevantExperience).HasPrecision(5, 2);
        builder.Property(e => e.EducationRequirement).HasPrecision(5, 2);
        builder.Property(e => e.AssessmentPerformance).HasPrecision(5, 2);
        builder.Property(e => e.InterviewPerformance).HasPrecision(5, 2);
        builder.Property(e => e.Communication).HasPrecision(5, 2);
        builder.Property(e => e.ProblemSolving).HasPrecision(5, 2);
        builder.Property(e => e.RoleReadiness).HasPrecision(5, 2);
        builder.Property(e => e.Strengths).HasMaxLength(4000);
        builder.Property(e => e.Weaknesses).HasMaxLength(4000);
        builder.Property(e => e.DocumentedRisks).HasMaxLength(4000);
        builder.Property(e => e.Justification).HasMaxLength(4000);
        builder.Property(e => e.Recommendation).HasMaxLength(500);

        builder.HasIndex(e => new { e.ApplicationId, e.EvaluatorUserId });

        builder.HasOne(e => e.Application)
            .WithMany()
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EvaluatorUser)
            .WithMany()
            .HasForeignKey(e => e.EvaluatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
