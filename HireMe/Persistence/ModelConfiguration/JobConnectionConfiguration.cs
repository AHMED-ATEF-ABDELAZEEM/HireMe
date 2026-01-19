using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class JobConnectionConfiguration : IEntityTypeConfiguration<JobConnection>
    {
        public void Configure(EntityTypeBuilder<JobConnection> builder)
        {
            builder.HasOne(jc => jc.Job)
                .WithMany()
                .HasForeignKey(jc => jc.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(jc => jc.Worker)
                .WithMany()
                .HasForeignKey(jc => jc.WorkerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(jc => jc.Employer)
                .WithMany()
                .HasForeignKey(jc => jc.EmployerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
