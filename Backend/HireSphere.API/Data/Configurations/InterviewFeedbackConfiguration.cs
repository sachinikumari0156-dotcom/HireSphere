using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class InterviewFeedbackConfiguration : IEntityTypeConfiguration<InterviewFeedback>
{
    public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
    {
        builder.Property(f => f.Rating).HasPrecision(5, 2);
        builder.Property(f => f.TechnicalCompetency).HasPrecision(5, 2);
        builder.Property(f => f.Communication).HasPrecision(5, 2);
        builder.Property(f => f.ProblemSolving).HasPrecision(5, 2);
        builder.Property(f => f.RoleKnowledge).HasPrecision(5, 2);
        builder.Property(f => f.Teamwork).HasPrecision(5, 2);
        builder.Property(f => f.Leadership).HasPrecision(5, 2);
        builder.Property(f => f.CulturalContribution).HasPrecision(5, 2);
        builder.Property(f => f.Comments).HasMaxLength(4000);
        builder.Property(f => f.Strengths).HasMaxLength(4000);
        builder.Property(f => f.Concerns).HasMaxLength(4000);
        builder.Property(f => f.Recommendation).HasMaxLength(500);
        builder.Property(f => f.PrivatePanelComments).HasMaxLength(4000);

        builder.HasIndex(f => new { f.InterviewId, f.InterviewerId }).IsUnique();

        builder.HasOne(f => f.Interviewer)
            .WithMany()
            .HasForeignKey(f => f.InterviewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
