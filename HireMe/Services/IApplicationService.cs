using HireMe.Contracts.Application.Requests;
using HireMe.Contracts.Application.Responses;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using HireMe.BackgroundJobs;
using HireMe.Contracts.Application;

namespace HireMe.Services
{
    public interface IApplicationService
    {
        Task<Result<Application>> AddApplicationAsync(string workerId, AddApplicationRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateApplicationAsync(string workerId, int applicationId, UpdateApplicationRequest request, CancellationToken cancellationToken = default);
        Task<Result> AcceptApplicationAsync(string employerId, int applicationId, CancellationToken cancellationToken = default);
        Task<Result> RejectApplicationAsync(string employerId, int applicationId, CancellationToken cancellationToken = default);
        Task<Result> WithdrawApplicationAsync(string workerId, int applicationId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AppliedApplicationResponse>>> GetAppliedApplicationsByJobIdAsync(string employerId, int jobId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<PendingApplicationResponse>>> GetPendingApplicationsForWorkerAsync(string workerId, CancellationToken cancellationToken = default);
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
        
        // TODO : send notification to employer that new application on job is created
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

        // TODO : send notification to worket that application on job is accepted
        // (Done) TODO : Add Background jobs when create JobConnection to complete it if not canceld afetr 10 day
        public async Task<Result> AcceptApplicationAsync(string employerId, int applicationId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting application acceptance process for application {ApplicationId} by employer {EmployerId}", applicationId, employerId);

            await  using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);


            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

            if (application is null)
            {
                _logger.LogWarning("Application acceptance failed: Application with ID {ApplicationId} not found", applicationId);
                return Result.Failure(ApplicationErrors.ApplicationNotFound);
            }

            if (application.Job is null || application.Job.EmployerId != employerId)
            {
                _logger.LogWarning("Application acceptance failed: Employer {EmployerId} does not own the job for application {ApplicationId}", employerId, applicationId);
                return Result.Failure(ApplicationErrors.JobNotOwnedByEmployer);
            }

            if (application.Job.Status != JobStatus.Published)
            {
                _logger.LogWarning("Application acceptance failed: Job {JobId} is not published (Status: {Status})", application.JobId, application.Job.Status);
                return Result.Failure(ApplicationErrors.JobNotAcceptingApplications);
            }

            if (application.Status != ApplicationStatus.Applied)
            {
                _logger.LogWarning("Application acceptance failed: Application {ApplicationId} is not in Applied status (Status: {Status})", applicationId, application.Status);
                return Result.Failure(ApplicationErrors.InvalidApplicationStatus);
            }

            // Check if worker already has an active connection
            var workerHasActiveConnection = await _context.JobConnections
                .AnyAsync(jc => jc.WorkerId == application.WorkerId && 
                               jc.Status == JobConnectionStatus.Active, 
                          cancellationToken);

            if (workerHasActiveConnection)
            {
                _logger.LogWarning("Application acceptance failed: Worker {WorkerId} already has an active job connection", application.WorkerId);
                return Result.Failure(ApplicationErrors.WorkerHasActiveConnection);
            }

            // Accept the application
            application.Status = ApplicationStatus.Accepted;
            application.StatusChangedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;
            
            application.Job.Status = JobStatus.InProgress;
            application.Job.UpdatedAt = DateTime.UtcNow;

            // Create JobConnection
            var jobConnection = new JobConnection
            {
                JobId = application.JobId,
                WorkerId = application.WorkerId,
                EmployerId = employerId,
                Status = JobConnectionStatus.Active,
                InteractionEndDate = DateTime.UtcNow.AddDays(10)
            };

            await _context.JobConnections.AddAsync(jobConnection, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Enqueue background job to handle related application status updates
            BackgroundJob.Enqueue<IApplicationStatusBackgroundJob>(job => 
                job.HandleApplicationAcceptanceAsync(application.JobId, applicationId, application.WorkerId));

            // Schedule background job to complete the job connection and process feedback at InteractionEndDate
            BackgroundJob.Schedule<IJobConnectionCompletionBackgroundJob>(
                job => job.ProcessJobConnectionCompletionAsync(jobConnection.Id),
                jobConnection.InteractionEndDate);

            _logger.LogInformation("Application {ApplicationId} accepted successfully by employer {EmployerId}. JobConnection {JobConnectionId} created. Background jobs enqueued.", 
                applicationId, employerId, jobConnection.Id);
            return Result.Success();
        }
        
        // TODO : send notification to worker that application on job is rejected
        public async Task<Result> RejectApplicationAsync(string employerId, int applicationId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting application rejection process for application {ApplicationId} by employer {EmployerId}", applicationId, employerId);

            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

            if (application is null)
            {
                _logger.LogWarning("Application rejection failed: Application with ID {ApplicationId} not found", applicationId);
                return Result.Failure(ApplicationErrors.ApplicationNotFound);
            }

            if (application.Status != ApplicationStatus.Applied)
            {
                _logger.LogWarning("Application rejection failed: Application {ApplicationId} is not in Applied status (Status: {Status})", applicationId, application.Status);
                return Result.Failure(ApplicationErrors.InvalidApplicationStatus);
            }

            if (application.Job is null || application.Job.EmployerId != employerId)
            {
                _logger.LogWarning("Application rejection failed: Employer {EmployerId} does not own the job for application {ApplicationId}", employerId, applicationId);
                return Result.Failure(ApplicationErrors.JobNotOwnedByEmployer);
            }



            application.Status = ApplicationStatus.Rejected;
            application.StatusChangedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} rejected successfully by employer {EmployerId}", applicationId, employerId);
            return Result.Success();
        }

        public async Task<Result> WithdrawApplicationAsync(string workerId, int applicationId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting application withdrawal process for application {ApplicationId} by worker {WorkerId}", applicationId, workerId);

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

            if (application is null)
            {
                _logger.LogWarning("Application withdrawal failed: Application with ID {ApplicationId} not found", applicationId);
                return Result.Failure(ApplicationErrors.ApplicationNotFound);
            }

            if (application.WorkerId != workerId)
            {
                _logger.LogWarning("Application withdrawal failed: Worker {WorkerId} is not authorized to withdraw application {ApplicationId}", workerId, applicationId);
                return Result.Failure(ApplicationErrors.UnauthorizedApplicationUpdate);
            }

            if (application.Status != ApplicationStatus.Applied)
            {
                _logger.LogWarning("Application withdrawal failed: Application {ApplicationId} is not in Applied status (Status: {Status})", applicationId, application.Status);
                return Result.Failure(ApplicationErrors.CannotUpdateApplication);
            }

            application.Status = ApplicationStatus.Withdrawn;
            application.StatusChangedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Application {ApplicationId} withdrawn successfully by worker {WorkerId}", applicationId, workerId);
            return Result.Success();
        }

       // TODO : return with worker information breif about history of jobs feadback,reports,completedjob,canceldjob
        public async Task<Result<IEnumerable<AppliedApplicationResponse>>> GetAppliedApplicationsByJobIdAsync(string employerId, int jobId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving applied applications for job {JobId} by employer {EmployerId}", jobId, employerId);

            var job = await _context.Jobs
                .Where(j => j.Id == jobId)
                .Select(j => new { j.Id, j.EmployerId })
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
            {
                _logger.LogWarning("Get applied applications failed: Job with ID {JobId} not found", jobId);
                return Result.Failure<IEnumerable<AppliedApplicationResponse>>(ApplicationErrors.JobNotFound);
            }

            if (job.EmployerId != employerId)
            {
                _logger.LogWarning("Get applied applications failed: Employer {EmployerId} does not own job {JobId}", employerId, jobId);
                return Result.Failure<IEnumerable<AppliedApplicationResponse>>(ApplicationErrors.JobNotOwnedByEmployer);
            }

            var applications = await _context.Applications
                .Where(a => a.JobId == jobId && a.Status == ApplicationStatus.Applied)
                .Include(a => a.Worker)
                .Select(a => new AppliedApplicationResponse
                {
                    ApplicationId = a.Id,
                    Message = a.Message ?? string.Empty,
                    CreatedAt = a.CreatedAt,
                    IsUpdated = a.UpdatedAt != null,
                    Worker = new WorkerInfoResponse
                    {
                        WorkerId = a.WorkerId,
                        FullName = a.Worker!.FirstName + " " + a.Worker.LastName,
                        ImageProfile = a.Worker.ImageProfile
                    }
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} applied applications for job {JobId}", applications.Count, jobId);
            return Result.Success<IEnumerable<AppliedApplicationResponse>>(applications);
        }

        public async Task<Result<IEnumerable<PendingApplicationResponse>>> GetPendingApplicationsForWorkerAsync(string workerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving pending applications for worker {WorkerId}", workerId);

            var pendingApplications = await _context.Applications
                .Where(a => a.WorkerId == workerId && a.Status == ApplicationStatus.Applied)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new PendingApplicationResponse(
                    a.Id,
                    a.Message,
                    a.CreatedAt,
                    a.Job.JobTitle,
                    a.Job.Salary,
                    a.Job.Governorate.NameArabic
                ))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} pending applications for worker {WorkerId}", pendingApplications.Count, workerId);
            return Result.Success<IEnumerable<PendingApplicationResponse>>(pendingApplications);
        }

    }
}
