using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.Property(c => c.FullName).HasMaxLength(200);
        builder.Property(c => c.PhoneNumber).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Skills).HasMaxLength(4000);
        builder.Property(c => c.ResumePath).HasMaxLength(500);
        builder.Property(c => c.Summary).HasMaxLength(4000);
        builder.Property(c => c.Location).HasMaxLength(200);
        builder.Property(c => c.DesiredJobTitle).HasMaxLength(200);
        builder.Property(c => c.Availability).HasMaxLength(200);
        builder.Property(c => c.PortfolioUrl).HasMaxLength(500);
        builder.Property(c => c.LinkedInUrl).HasMaxLength(500);
        builder.Property(c => c.GitHubUrl).HasMaxLength(500);

        builder.HasMany(c => c.WorkExperiences)
            .WithOne(w => w.CandidateProfile)
            .HasForeignKey(w => w.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Educations)
            .WithOne(e => e.CandidateProfile)
            .HasForeignKey(e => e.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CandidateSkills)
            .WithOne(cs => cs.CandidateProfile)
            .HasForeignKey(cs => cs.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Certifications)
            .WithOne(cert => cert.CandidateProfile)
            .HasForeignKey(cert => cert.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Resumes)
            .WithOne(r => r.CandidateProfile)
            .HasForeignKey(r => r.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documents)
            .WithOne(d => d.CandidateProfile)
            .HasForeignKey(d => d.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
