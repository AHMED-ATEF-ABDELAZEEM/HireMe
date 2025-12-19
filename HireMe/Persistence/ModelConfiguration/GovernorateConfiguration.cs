using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.ModelConfiguration
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.Property(g => g.NameArabic)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(g => g.NameEnglish)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
    
}
