using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Persistence;
using HireMe.SeedingData;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IJobConnectionService
    {
        Task<Result> CancelJobConnectionAsync(string userId, int jobConnectionId, IEnumerable<string> userRoles, CancellationToken cancellationToken = default);
    }

    public class JobConnectionService : IJobConnectionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobConnectionService> _logger;

        public JobConnectionService(AppDbContext context, ILogger<JobConnectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> CancelJobConnectionAsync(string userId, int jobConnectionId, IEnumerable<string> userRoles, CancellationToken cancellationToken = default)
        {
            // Determine UserType from roles
            var userType = GetUserTypeFromRoles(userRoles);
            
            if (userType == UserType.UnDefined)
            {
                _logger.LogWarning("User {UserId} has undefined user type", userId);
                return Result.Failure(CustomErrors.JobConnectionErrors.UnauthorizedCancellation);
            }
            
            _logger.LogInformation("User {UserId} attempting to cancel job connection {JobConnectionId} as {UserType}", userId, jobConnectionId, userType);

            // Retrieve the job connection with related job
            var jobConnection = await _context.JobConnections
                .Include(jc => jc.Job)
                .FirstOrDefaultAsync(jc => jc.Id == jobConnectionId, cancellationToken);

            if (jobConnection is null)
            {
                _logger.LogWarning("Job connection {JobConnectionId} not found", jobConnectionId);
                return Result.Failure(CustomErrors.JobConnectionErrors.JobConnectionNotFound);
            }

            // Check if status is Active
            if (jobConnection.Status != JobConnectionStatus.Active)
            {
                _logger.LogWarning("Job connection {JobConnectionId} is not active (Status: {Status})", jobConnectionId, jobConnection.Status);
                return Result.Failure(CustomErrors.JobConnectionErrors.JobConnectionNotActive);
            }

            // Verify user has permission to cancel (is either the worker or employer)
            bool isAuthorized = userType switch
            {
                UserType.Worker => jobConnection.WorkerId == userId,
                UserType.Employer => jobConnection.EmployerId == userId,
                _ => false
            };

            if (!isAuthorized)
            {
                _logger.LogWarning("User {UserId} is not authorized to cancel job connection {JobConnectionId}", userId, jobConnectionId);
                return Result.Failure(CustomErrors.JobConnectionErrors.UnauthorizedCancellation);
            }

            // Update job connection status based on who is cancelling
            jobConnection.Status = userType == UserType.Worker 
                ? JobConnectionStatus.CancelledByWorker 
                : JobConnectionStatus.CancelledByEmployer;
            
            jobConnection.CancelledAt = DateTime.UtcNow;

            // Update job status to Cancelled
            if (jobConnection.Job != null)
            {
                jobConnection.Job.Status = JobStatus.Cancelled;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully cancelled job connection {JobConnectionId} by {UserType}. Job {JobId} status updated to Cancelled.", 
                jobConnectionId, userType, jobConnection.JobId);

            return Result.Success();
        }

        private UserType GetUserTypeFromRoles(IEnumerable<string> roles)
        {
            if (roles.Contains(DefaultRoles.Worker))
            {
                return UserType.Worker;
            }
            else if (roles.Contains(DefaultRoles.Employer))
            {
                return UserType.Employer;
            }
            
            return UserType.UnDefined;
        }
    }
}
