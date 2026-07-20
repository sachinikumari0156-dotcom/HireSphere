using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class InterviewFeedbackConfiguration : IEntityTypeConfiguration<InterviewFeedback>
{
    public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
    {
        builder.Property(f => f.Rating).HasPrecision(5, 2);
        builder.Property(f => f.Comments).HasMaxLength(4000);

        builder.HasOne(f => f.Interviewer)
            .WithMany()
            .HasForeignKey(f => f.InterviewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
