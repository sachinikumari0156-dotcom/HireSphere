using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AIInsightConfiguration : IEntityTypeConfiguration<AIInsight>
{
    public void Configure(EntityTypeBuilder<AIInsight> builder)
    {
        builder.Property(i => i.InsightType).HasMaxLength(100);
        builder.Property(i => i.Content).HasMaxLength(4000);
        builder.Property(i => i.ConfidenceScore).HasPrecision(5, 2);

        builder.HasOne(i => i.Application)
            .WithMany()
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.CandidateProfile)
            .WithMany()
            .HasForeignKey(i => i.CandidateProfileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
