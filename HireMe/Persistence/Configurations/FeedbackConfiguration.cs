using HireMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireMe.Persistence.Configurations
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Rating)
                .IsRequired()
                .HasComment("Rating from 1 to 5 stars");

            builder.Property(f => f.Message)
                .HasMaxLength(500);

            builder.Property(f => f.IsVisible)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Set to true by background job after InteractionEndDate");

            // Relationships
            builder.HasOne(f => f.JobConnection)
                .WithMany()
                .HasForeignKey(f => f.JobConnectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.FromUser)
                .WithMany()
                .HasForeignKey(f => f.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.ToUser)
                .WithMany()
                .HasForeignKey(f => f.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(f => f.JobConnectionId);
            builder.HasIndex(f => f.FromUserId);
            builder.HasIndex(f => f.ToUserId);
            builder.HasIndex(f => f.IsVisible);

            // Unique constraint: One feedback per user per connection
            builder.HasIndex(f => new { f.JobConnectionId, f.FromUserId })
                .IsUnique()
                .HasDatabaseName("IX_Feedback_JobConnection_FromUser_Unique");
        }
    }
}
