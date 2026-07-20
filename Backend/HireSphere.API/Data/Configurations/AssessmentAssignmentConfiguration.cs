using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AssessmentAssignmentConfiguration : IEntityTypeConfiguration<AssessmentAssignment>
{
    public void Configure(EntityTypeBuilder<AssessmentAssignment> builder)
    {
        builder.HasIndex(a => new { a.CandidateId, a.SkillAssessmentId });

        builder.HasOne(a => a.Candidate)
            .WithMany()
            .HasForeignKey(a => a.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Application)
            .WithMany()
            .HasForeignKey(a => a.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Attempts)
            .WithOne(at => at.AssessmentAssignment)
            .HasForeignKey(at => at.AssessmentAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
