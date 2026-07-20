using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => new { a.CandidateId, a.JobId }).IsUnique();

        builder.Property(a => a.CoverLetter).HasMaxLength(4000);

        builder.HasOne(a => a.Resume)
            .WithMany()
            .HasForeignKey(a => a.ResumeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.Application)
            .HasForeignKey(ans => ans.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.StatusHistory)
            .WithOne(sh => sh.Application)
            .HasForeignKey(sh => sh.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
