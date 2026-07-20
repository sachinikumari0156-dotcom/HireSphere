using HireSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireSphere.API.Data.Configurations;

public class ApplicationAnswerConfiguration : IEntityTypeConfiguration<ApplicationAnswer>
{
    public void Configure(EntityTypeBuilder<ApplicationAnswer> builder)
    {
        builder.Property(a => a.QuestionText).HasMaxLength(4000);
        builder.Property(a => a.AnswerText).HasMaxLength(4000);

        builder.HasOne(a => a.ScreeningQuestion)
            .WithMany(sq => sq.Answers)
            .HasForeignKey(a => a.ScreeningQuestionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
