using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.Action).HasMaxLength(200);
        builder.Property(a => a.EntityType).HasMaxLength(200);
        builder.Property(a => a.Details).HasMaxLength(4000);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.ActorRole).HasMaxLength(50);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
