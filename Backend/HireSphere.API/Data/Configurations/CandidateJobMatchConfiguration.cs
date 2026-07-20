using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CandidateJobMatchConfiguration : IEntityTypeConfiguration<CandidateJobMatch>
{
    public void Configure(EntityTypeBuilder<CandidateJobMatch> builder)
    {
        builder.Property(m => m.MatchScore).HasPrecision(5, 2);
        builder.Property(m => m.MatchSummary).HasMaxLength(4000);

        builder.HasOne(m => m.CandidateProfile)
            .WithMany()
            .HasForeignKey(m => m.CandidateProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Job)
            .WithMany()
            .HasForeignKey(m => m.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
