using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.Property(j => j.JobTitle)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(j => j.Salary)
                .HasPrecision(18, 2);

            builder.Property(j => j.Address)
                .HasMaxLength(100);

            builder.Property(j => j.Description)
                .HasMaxLength(500);
                
            builder.Property(j => j.Experience)
                .HasMaxLength(500);
        }
    }
    
}
