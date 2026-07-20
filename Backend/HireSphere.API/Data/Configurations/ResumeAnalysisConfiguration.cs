using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.Property(r => r.AnalysisSummary).HasMaxLength(4000);
        builder.Property(r => r.OverallScore).HasPrecision(5, 2);

        builder.HasOne(r => r.Resume)
            .WithMany()
            .HasForeignKey(r => r.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
