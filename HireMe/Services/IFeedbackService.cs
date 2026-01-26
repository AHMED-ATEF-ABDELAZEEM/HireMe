using HireMe.Contracts.Feedback;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IFeedbackService
    {
        Task<Result> AddFeedbackAsync(string fromUserId, AddFeedbackRequest request, CancellationToken cancellationToken = default);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(AppDbContext context, ILogger<FeedbackService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> AddFeedbackAsync(string fromUserId, AddFeedbackRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting feedback creation for JobConnection {JobConnectionId} by user {FromUserId}", request.JobConnectionId, fromUserId);

            // Get the job connection with employer and worker info
            var jobConnection = await _context.JobConnections
                .FirstOrDefaultAsync(jc => jc.Id == request.JobConnectionId, cancellationToken);

            if (jobConnection is null)
            {
                _logger.LogWarning("Feedback creation failed: JobConnection {JobConnectionId} not found", request.JobConnectionId);
                return Result.Failure(FeedbackErrors.JobConnectionNotFound);
            }

            // Check if interaction period has ended
            if (DateTime.UtcNow >= jobConnection.InteractionEndDate)
            {
                _logger.LogWarning("Feedback creation failed: Interaction period for JobConnection {JobConnectionId} has ended", request.JobConnectionId);
                return Result.Failure(FeedbackErrors.InteractionPeriodEnded);
            }

            // Verify user is part of this connection
            if (jobConnection.EmployerId != fromUserId && jobConnection.WorkerId != fromUserId)
            {
                _logger.LogWarning("Feedback creation failed: User {FromUserId} is not part of JobConnection {JobConnectionId}", fromUserId, request.JobConnectionId);
                return Result.Failure(FeedbackErrors.NotPartOfConnection);
            }



            // Determine who is receiving the feedback
            string toUserId = jobConnection.EmployerId == fromUserId 
                ? jobConnection.WorkerId 
                : jobConnection.EmployerId;

            // Check if user already submitted feedback for this connection
            var existingFeedback = await _context.Feedbacks
                .AnyAsync(f => f.JobConnectionId == request.JobConnectionId && f.FromUserId == fromUserId, cancellationToken);

            if (existingFeedback)
            {
                _logger.LogWarning("Feedback creation failed: User {FromUserId} has already submitted feedback for JobConnection {JobConnectionId}", fromUserId, request.JobConnectionId);
                return Result.Failure(FeedbackErrors.FeedbackAlreadyExists);
            }

            // Create the feedback
            var feedback = new Models.Feedback
            {
                JobConnectionId = request.JobConnectionId,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Rating = request.Rating,
                Message = request.Message,
                IsVisible = false // Will be set to true by background job
            };

            await _context.Feedbacks.AddAsync(feedback, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Feedback created successfully with ID {FeedbackId} for JobConnection {JobConnectionId}", feedback.Id, request.JobConnectionId);

            return Result.Success();
        }
    }
}
