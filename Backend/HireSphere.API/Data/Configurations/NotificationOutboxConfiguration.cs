using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.Property(x => x.NotificationType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DestinationMasked).HasMaxLength(100);
        builder.Property(x => x.Provider).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.Property(x => x.SafeFailureCode).HasMaxLength(100);
        builder.Property(x => x.BodySummary).HasMaxLength(500);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.Status, x.QueuedAtUtc });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
