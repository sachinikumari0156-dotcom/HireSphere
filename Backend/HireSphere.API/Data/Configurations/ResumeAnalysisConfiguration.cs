using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.Property(r => r.AnalysisSummary).HasMaxLength(4000);
        builder.Property(r => r.OverallScore).HasPrecision(5, 2);
        builder.Property(r => r.Provider).HasMaxLength(100).IsRequired();
        builder.Property(r => r.ProviderVersion).HasMaxLength(100).IsRequired();
        builder.Property(r => r.ProviderType).HasMaxLength(50).IsRequired();
        builder.Property(r => r.FailureReason).HasMaxLength(500);
        builder.Property(r => r.ExtractedName).HasMaxLength(200);
        builder.Property(r => r.ExtractedEmail).HasMaxLength(256);
        builder.Property(r => r.ExtractedPhone).HasMaxLength(50);
        builder.Property(r => r.ExtractedSummary).HasMaxLength(2000);
        builder.Property(r => r.FallbackNote).HasMaxLength(500);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(40);

        builder.HasOne(r => r.Resume)
            .WithMany()
            .HasForeignKey(r => r.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ResumeId);
    }
}

public class ExtractedSkillConfiguration : IEntityTypeConfiguration<ExtractedSkill>
{
    public void Configure(EntityTypeBuilder<ExtractedSkill> builder)
    {
        builder.Property(s => s.RawName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.CanonicalName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Confidence).HasPrecision(5, 2);
        builder.Property(s => s.SourceEvidence).HasMaxLength(200);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(40);

        builder.HasOne(s => s.ResumeAnalysis)
            .WithMany(a => a.ExtractedSkills)
            .HasForeignKey(s => s.ResumeAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ResumeAnalysisId, s.CanonicalName });
    }
}
