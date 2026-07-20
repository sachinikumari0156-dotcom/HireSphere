using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.Title);

        builder.Property(j => j.Title).HasMaxLength(200);
        builder.Property(j => j.Description).HasMaxLength(4000);
        builder.Property(j => j.Responsibilities).HasMaxLength(4000);
        builder.Property(j => j.RequiredSkills).HasMaxLength(4000);
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.JobType).HasMaxLength(50);
        builder.Property(j => j.SalaryCurrency).HasMaxLength(10);
        builder.Property(j => j.EducationRequirement).HasMaxLength(500);
        builder.Property(j => j.SalaryMin).HasPrecision(18, 2);
        builder.Property(j => j.SalaryMax).HasPrecision(18, 2);

        builder.HasOne(j => j.Organization)
            .WithMany(o => o.Jobs)
            .HasForeignKey(j => j.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.Department)
            .WithMany(d => d.Jobs)
            .HasForeignKey(j => j.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.HiringManager)
            .WithMany()
            .HasForeignKey(j => j.HiringManagerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.JobSkills)
            .WithOne(js => js.Job)
            .HasForeignKey(js => js.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.ScreeningQuestions)
            .WithOne(sq => sq.Job)
            .HasForeignKey(sq => sq.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.Applications)
            .WithOne(a => a.Job)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
