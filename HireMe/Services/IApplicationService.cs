using HireMe.Contracts.Application.Requests;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IApplicationService
    {
        Task<Result<Application>> AddApplicationAsync(string workerId, AddApplicationRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateApplicationAsync(string workerId, int applicationId, UpdateApplicationRequest request, CancellationToken cancellationToken = default);
    }

    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(AppDbContext context, ILogger<ApplicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Application>> AddApplicationAsync(string workerId, AddApplicationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting application creation process for user {WorkerId} on job {JobId}", workerId, request.JobId);

            var job = await _context.Jobs
                .Where(j => j.Id == request.JobId)
                .Select(j => new { j.Id, j.Status })
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
            {
                _logger.LogWarning("Application creation failed: Job with ID {JobId} not found", request.JobId);
                return Result.Failure<Application>(ApplicationErrors.JobNotFound);
            }

            if (job.Status != JobStatus.Published)
            {
                _logger.LogWarning("Application creation failed: Job with ID {JobId} is not published (Status: {Status})", request.JobId, job.Status);
                return Result.Failure<Application>(ApplicationErrors.JobNotAcceptingApplications);
            }


            var existingApplication = await _context.Applications
                .AnyAsync(a => a.JobId == request.JobId && a.WorkerId == workerId, cancellationToken);

            if (existingApplication)
            {
                _logger.LogWarning("Application creation failed: User {WorkerId} has already applied to job {JobId}", workerId, request.JobId);
                return Result.Failure<Application>(ApplicationErrors.AlreadyApplied);
            }

            var application = new Application
            {
                JobId = request.JobId,
                WorkerId = workerId,
                Message = request.Message,
            };

            await _context.Applications.AddAsync(application, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Application created successfully with ID: {ApplicationId} for job {JobId}", application.Id, request.JobId);
            return Result.Success(application);
        }

        public async Task<Result> UpdateApplicationAsync(string workerId, int applicationId, UpdateApplicationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting application update process for application {ApplicationId} by user {WorkerId}", applicationId, workerId);

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

            if (application is null)
            {
                _logger.LogWarning("Application update failed: Application with ID {ApplicationId} not found", applicationId);
                return Result.Failure(ApplicationErrors.ApplicationNotFound);
            }

            if (application.WorkerId != workerId)
            {
                _logger.LogWarning("Application update failed: User {WorkerId} is not authorized to update application {ApplicationId}", workerId, applicationId);
                return Result.Failure(ApplicationErrors.UnauthorizedApplicationUpdate);
            }

            if (application.Status != ApplicationStatus.Applied)
            {
                _logger.LogWarning("Application update failed: Application {ApplicationId} is not in Applied status (Status: {Status})", applicationId, application.Status);
                return Result.Failure(ApplicationErrors.CannotUpdateApplication);
            }

            application.Message = request.Message;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Application updated successfully with ID: {ApplicationId}", application.Id);
            return Result.Success();
        }
    }
}
