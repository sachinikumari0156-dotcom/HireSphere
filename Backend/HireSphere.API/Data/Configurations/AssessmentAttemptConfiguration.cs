using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AssessmentAttemptConfiguration : IEntityTypeConfiguration<AssessmentAttempt>
{
    public void Configure(EntityTypeBuilder<AssessmentAttempt> builder)
    {
        builder.HasOne(at => at.Candidate)
            .WithMany()
            .HasForeignKey(at => at.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(at => at.Result)
            .WithOne(r => r.AssessmentAttempt)
            .HasForeignKey<AssessmentResult>(r => r.AssessmentAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
