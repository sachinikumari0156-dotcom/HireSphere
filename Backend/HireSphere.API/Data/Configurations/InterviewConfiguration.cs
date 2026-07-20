using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.Property(i => i.InterviewType).HasMaxLength(100);
        builder.Property(i => i.MeetingLink).HasMaxLength(500);
        builder.Property(i => i.TimeZoneId).HasMaxLength(100);
        builder.Property(i => i.MeetingInstructions).HasMaxLength(2000);
        builder.Property(i => i.CandidateResponseReason).HasMaxLength(2000);
        builder.Property(i => i.PhysicalLocation).HasMaxLength(500);
        builder.Property(i => i.InternalNotes).HasMaxLength(4000);
        builder.Property(i => i.CalendarSyncStatus).HasMaxLength(50);

        builder.HasOne(i => i.Application)
            .WithMany(a => a.Interviews)
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Recruiter)
            .WithMany()
            .HasForeignKey(i => i.RecruiterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.HiringManager)
            .WithMany()
            .HasForeignKey(i => i.HiringManagerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(i => i.EndsAtUtc);

        builder.HasMany(i => i.Participants)
            .WithOne(p => p.Interview)
            .HasForeignKey(p => p.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Feedbacks)
            .WithOne(f => f.Interview)
            .HasForeignKey(f => f.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
