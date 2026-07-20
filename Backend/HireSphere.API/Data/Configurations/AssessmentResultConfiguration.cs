using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AssessmentResultConfiguration : IEntityTypeConfiguration<AssessmentResult>
{
    public void Configure(EntityTypeBuilder<AssessmentResult> builder)
    {
        builder.HasIndex(r => r.AssessmentAttemptId).IsUnique();

        builder.Property(r => r.Score).HasPrecision(8, 2);
        builder.Property(r => r.MaxScore).HasPrecision(8, 2);
        builder.Property(r => r.Feedback).HasMaxLength(4000);
    }
}
