using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CandidateDocumentConfiguration : IEntityTypeConfiguration<CandidateDocument>
{
    public void Configure(EntityTypeBuilder<CandidateDocument> builder)
    {
        builder.Property(d => d.FilePath).HasMaxLength(1000);
        builder.Property(d => d.FileName).HasMaxLength(255);
        builder.Property(d => d.ContentType).HasMaxLength(150);
        builder.Property(d => d.ChecksumSha256).HasMaxLength(64);
        builder.Property(d => d.ValidationStatus).HasMaxLength(50);
        builder.Property(d => d.ScanStatus).HasMaxLength(50);
        builder.Property(d => d.StorageProvider).HasMaxLength(50);
    }
}
