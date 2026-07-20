using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.Property(q => q.QuestionText).HasMaxLength(4000);
        builder.Property(q => q.QuestionType).HasMaxLength(50);
        builder.Property(q => q.Points).HasPrecision(5, 2);
        builder.Property(q => q.OptionsJson).HasMaxLength(4000);
        builder.Property(q => q.CorrectAnswerKey).HasMaxLength(500);
    }
}
