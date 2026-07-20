using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AssessmentAnswerConfiguration : IEntityTypeConfiguration<AssessmentAnswer>
{
    public void Configure(EntityTypeBuilder<AssessmentAnswer> builder)
    {
        builder.Property(a => a.AnswerValue).HasMaxLength(4000);
        builder.Property(a => a.AwardedPoints).HasPrecision(5, 2);

        builder.HasIndex(a => new { a.AssessmentAttemptId, a.AssessmentQuestionId }).IsUnique();

        builder.HasOne(a => a.AssessmentQuestion)
            .WithMany()
            .HasForeignKey(a => a.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
