using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.BackgroundJobs
{
    public interface IJobConnectionCompletionBackgroundJob
    {
        Task ProcessJobConnectionCompletionAsync(int jobConnectionId);
    }

    public class JobConnectionCompletionBackgroundJob : IJobConnectionCompletionBackgroundJob
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobConnectionCompletionBackgroundJob> _logger;

        public JobConnectionCompletionBackgroundJob(AppDbContext context, ILogger<JobConnectionCompletionBackgroundJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        // TODO: send notification to users about feedback visibility and rating updates

        public async Task ProcessJobConnectionCompletionAsync(int jobConnectionId)
        {
            try
            {
                _logger.LogInformation("Starting job connection completion process for JobConnection {JobConnectionId}", jobConnectionId);

                var jobConnection = await _context.JobConnections
                    .Include(jc => jc.Job)
                    .FirstOrDefaultAsync(jc => jc.Id == jobConnectionId);

                if (jobConnection is null)
                {
                    _logger.LogWarning("JobConnection {JobConnectionId} not found", jobConnectionId);
                    return;
                }

                // Update JobConnection status to Completed only if it's Active (not cancelled)
                if (jobConnection.Status == JobConnectionStatus.Active)
                {
                    jobConnection.Status = JobConnectionStatus.Completed;
                    jobConnection.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("JobConnection {JobConnectionId} marked as Completed", jobConnectionId);

                    // Update Job status to Completed as well
                    if (jobConnection.Job is not null)
                    {
                        jobConnection.Job.Status = JobStatus.Completed;
                        jobConnection.Job.UpdatedAt = DateTime.UtcNow;
                        _logger.LogInformation("Job {JobId} marked as Completed", jobConnection.JobId);
                    }
                }
                else
                {
                    _logger.LogInformation("JobConnection {JobConnectionId} status is {Status}. Status will not be changed to Completed, but feedback processing will continue.", 
                        jobConnectionId, jobConnection.Status);
                }

                // Check if feedbacks exist for this job connection
                var feedbacks = await _context.Feedbacks
                    .Where(f => f.JobConnectionId == jobConnectionId)
                    .ToListAsync();

                if (feedbacks.Any())
                {
                    _logger.LogInformation("Found {FeedbackCount} feedback(s) for JobConnection {JobConnectionId}", feedbacks.Count, jobConnectionId);

                    // Make all feedbacks visible
                    foreach (var feedback in feedbacks)
                    {
                        feedback.IsVisible = true;
                        feedback.UpdatedAt = DateTime.UtcNow;
                    }

                    // Calculate and update ratings for users who received feedback
                    await UpdateUserRatingsAsync(feedbacks);

                    _logger.LogInformation("All feedbacks for JobConnection {JobConnectionId} are now visible and ratings have been updated", jobConnectionId);
                }
                else
                {
                    _logger.LogInformation("No feedbacks found for JobConnection {JobConnectionId}", jobConnectionId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully completed processing for JobConnection {JobConnectionId}", jobConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing job connection completion for JobConnection {JobConnectionId}", jobConnectionId);
                throw;
            }
        }

        private async Task UpdateUserRatingsAsync(List<Feedback> feedbacks)
        {
            // Group feedbacks by ToUserId (each user can receive max one feedback per JobConnection)
            var feedbacksByUser = feedbacks.GroupBy(f => f.ToUserId);

            foreach (var userFeedbacks in feedbacksByUser)
            {
                var userId = userFeedbacks.Key;
                var feedback = userFeedbacks.Single(); // Only one feedback per user per JobConnection

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user is not null)
                {
                    // Add the single rating to existing totals
                    user.TotalRatingSum += feedback.Rating;
                    user.TotalRatingsCount += 1;
                    user.AverageRating = user.TotalRatingsCount > 0 
                        ? (double)user.TotalRatingSum / user.TotalRatingsCount 
                        : 0.0;

                    _logger.LogInformation("Updated ratings for user {UserId}: Added rating {Rating}, TotalSum={TotalSum}, Count={Count}, Average={Average:F2}", 
                        userId, feedback.Rating, user.TotalRatingSum, user.TotalRatingsCount, user.AverageRating);
                }
                else
                {
                    _logger.LogWarning("User {UserId} not found while updating ratings", userId);
                }
            }
        }
    }
}
