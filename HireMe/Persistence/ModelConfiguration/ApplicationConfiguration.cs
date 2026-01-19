using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Worker)
                .WithMany()
                .HasForeignKey(a => a.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
