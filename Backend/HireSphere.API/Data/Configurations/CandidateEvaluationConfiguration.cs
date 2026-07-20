using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CandidateEvaluationConfiguration : IEntityTypeConfiguration<CandidateEvaluation>
{
    public void Configure(EntityTypeBuilder<CandidateEvaluation> builder)
    {
        builder.Property(e => e.OverallScore).HasPrecision(5, 2);
        builder.Property(e => e.Strengths).HasMaxLength(4000);
        builder.Property(e => e.Weaknesses).HasMaxLength(4000);
        builder.Property(e => e.Recommendation).HasMaxLength(500);

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
