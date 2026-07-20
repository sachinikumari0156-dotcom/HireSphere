using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ApplicationMessageConfiguration : IEntityTypeConfiguration<ApplicationMessage>
{
    public void Configure(EntityTypeBuilder<ApplicationMessage> builder)
    {
        builder.Property(m => m.Body).HasMaxLength(4000).IsRequired();
        builder.Property(m => m.SenderRole).HasMaxLength(50).IsRequired();

        builder.HasOne(m => m.Application)
            .WithMany(a => a.Messages)
            .HasForeignKey(m => m.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ApplicationId, m.SentAtUtc });
    }
}

public class RankingReviewConfiguration : IEntityTypeConfiguration<RankingReview>
{
    public void Configure(EntityTypeBuilder<RankingReview> builder)
    {
        builder.Property(r => r.Decision).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(4000).IsRequired();
        builder.Property(r => r.OverrideScore).HasPrecision(5, 2);

        builder.HasOne(r => r.Application)
            .WithMany()
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ApplicationId);
    }
}
