using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.IssuingOrganization).HasMaxLength(200);
        builder.Property(c => c.CredentialId).HasMaxLength(200);
        builder.Property(c => c.CredentialUrl).HasMaxLength(500);
    }
}
