using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class RecruiterAccessRequestConfiguration : IEntityTypeConfiguration<RecruiterAccessRequest>
{
    public void Configure(EntityTypeBuilder<RecruiterAccessRequest> builder)
    {
        builder.Property(r => r.FullName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.BusinessEmail).HasMaxLength(256).IsRequired();
        builder.Property(r => r.NormalizedBusinessEmail).HasMaxLength(256).IsRequired();
        builder.Property(r => r.OrganizationName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Message).HasMaxLength(2000);
        builder.Property(r => r.ReviewNotes).HasMaxLength(2000);

        builder.HasIndex(r => r.NormalizedBusinessEmail);
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
