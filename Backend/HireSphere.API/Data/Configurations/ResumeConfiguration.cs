using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.Property(r => r.FilePath).HasMaxLength(1000);
        builder.Property(r => r.FileName).HasMaxLength(255);
        builder.Property(r => r.ContentType).HasMaxLength(150);
        builder.Property(r => r.ChecksumSha256).HasMaxLength(64);
        builder.Property(r => r.ValidationStatus).HasMaxLength(50);
        builder.Property(r => r.ScanStatus).HasMaxLength(50);
        builder.Property(r => r.StorageProvider).HasMaxLength(50);
    }
}
