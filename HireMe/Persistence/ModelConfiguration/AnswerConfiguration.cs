using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasOne(a => a.Question)
                .WithOne(q => q.Answer)
                .HasForeignKey<Answer>(a => a.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Employer)
                .WithMany()
                .HasForeignKey(a => a.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
