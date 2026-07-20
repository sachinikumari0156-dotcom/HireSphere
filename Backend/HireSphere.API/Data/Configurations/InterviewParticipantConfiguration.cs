using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class InterviewParticipantConfiguration : IEntityTypeConfiguration<InterviewParticipant>
{
    public void Configure(EntityTypeBuilder<InterviewParticipant> builder)
    {
        builder.Property(p => p.ParticipantRole).HasMaxLength(100);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
