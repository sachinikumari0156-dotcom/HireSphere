using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class HiringDecisionConfiguration : IEntityTypeConfiguration<HiringDecision>
{
    public void Configure(EntityTypeBuilder<HiringDecision> builder)
    {
        builder.Property(h => h.Notes).HasMaxLength(4000);
        builder.Property(h => h.Reason).HasMaxLength(2000).IsRequired();

        builder.HasIndex(h => h.ApplicationId);

        builder.HasOne(h => h.Application)
            .WithMany()
            .HasForeignKey(h => h.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.DecisionByUser)
            .WithMany()
            .HasForeignKey(h => h.DecisionByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
