using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class JobReviewCommentConfiguration : IEntityTypeConfiguration<JobReviewComment>
{
    public void Configure(EntityTypeBuilder<JobReviewComment> builder)
    {
        builder.Property(c => c.Content).HasMaxLength(4000).IsRequired();

        builder.HasOne(c => c.Job)
            .WithMany()
            .HasForeignKey(c => c.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.JobId);
    }
}
