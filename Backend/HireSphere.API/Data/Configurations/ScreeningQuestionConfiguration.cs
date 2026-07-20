using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ScreeningQuestionConfiguration : IEntityTypeConfiguration<ScreeningQuestion>
{
    public void Configure(EntityTypeBuilder<ScreeningQuestion> builder)
    {
        builder.Property(sq => sq.QuestionText).HasMaxLength(4000);
        builder.Property(sq => sq.QuestionType).HasMaxLength(50);
    }
}
