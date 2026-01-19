using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(q => q.Job)
                .WithMany(j => j.Questions)
                .HasForeignKey(q => q.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(q => q.Worker)
                .WithMany()
                .HasForeignKey(q => q.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
