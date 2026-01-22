using HireMe.Contracts.EmployerDashboard.Responses;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IEmployerDashboardService
    {
        Task<Result<JobAnalyticsResponse>> GetJobAnalyticsAsync(string employerId, int jobId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<RecentJobsSummaryResponse>>> GetRecentJobsAsync(string employerId, CancellationToken cancellationToken = default);
    }

    public class EmployerDashboardService : IEmployerDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployerDashboardService> _logger;

        public EmployerDashboardService(AppDbContext context, ILogger<EmployerDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<JobAnalyticsResponse>> GetJobAnalyticsAsync(string employerId, int jobId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting analytics aggregation for job {JobId} by employer {EmployerId}", jobId, employerId);

            var job = await _context.Jobs
                .Where(j => j.Id == jobId)
                .Select(j => new { j.Id, j.EmployerId, j.Status })
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
            {
                _logger.LogWarning("Analytics failed: Job with ID {JobId} not found", jobId);
                return Result.Failure<JobAnalyticsResponse>(JobErrors.JobNotFound);
            }

            if (job.EmployerId != employerId)
            {
                _logger.LogWarning("Analytics failed: Employer {EmployerId} does not own job {JobId}", employerId, jobId);
                return Result.Failure<JobAnalyticsResponse>(ApplicationErrors.JobNotOwnedByEmployer);
            }

            // Applications counts
            var applications = _context.Applications.AsNoTracking().Where(a => a.JobId == jobId);

            var totalApplicationsTask = applications.CountAsync(cancellationToken);
            var appliedCountTask = applications.Where(a => a.Status == ApplicationStatus.Applied).CountAsync(cancellationToken);
            var rejectedCountTask = applications.Where(a => a.Status == ApplicationStatus.Rejected).CountAsync(cancellationToken);
            var withdrawnCountTask = applications.Where(a => a.Status == ApplicationStatus.Withdrawn).CountAsync(cancellationToken);
            var acceptedAtAnotherJobCountTask = applications.Where(a => a.Status == ApplicationStatus.WorkerAcceptedAtAnotherJob).CountAsync(cancellationToken);
            var lastApplicationAtTask = applications.OrderByDescending(a => a.CreatedAt).Select(a => a.CreatedAt).FirstOrDefaultAsync(cancellationToken);

            // Unanswered questions
            var unansweredQuestionsTask = _context.Questions
                .AsNoTracking()
                .Where(q => q.JobId == jobId && q.Answer == null)
                .CountAsync(cancellationToken);

            // Active connection (if any)
            var activeConnectionTask = _context.JobConnections
                .AsNoTracking()
                .Include(jc => jc.Worker)
                .Where(jc => jc.JobId == jobId && jc.Status == JobConnectionStatus.Active)
                .Select(jc => new JobConnectionBriefResponse
                {
                    JobConnectionId = jc.Id,
                    StartedAt = jc.CreatedAt,
                    EndsAt = jc.InteractionEndDate,
                    Worker = new Contracts.Application.Responses.WorkerInfoResponse
                    {
                        WorkerId = jc.WorkerId,
                        FullName = jc.Worker!.FirstName + " " + jc.Worker.LastName,
                        ImageProfile = jc.Worker.ImageProfile
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            await Task.WhenAll(totalApplicationsTask, appliedCountTask, rejectedCountTask, withdrawnCountTask, acceptedAtAnotherJobCountTask, lastApplicationAtTask, unansweredQuestionsTask, activeConnectionTask);

            var response = new JobAnalyticsResponse
            {
                JobId = jobId,
                JobStatus = job.Status.ToString(),
                NumberOfApplications = totalApplicationsTask.Result,
                LastApplicationAt = lastApplicationAtTask.Result == default ? null : lastApplicationAtTask.Result,
                AppliedApplications = appliedCountTask.Result,
                RejectedApplications = rejectedCountTask.Result,
                WithdrawnApplications = withdrawnCountTask.Result,
                AcceptedAtAnotherJobApplications = acceptedAtAnotherJobCountTask.Result,
                UnansweredQuestions = unansweredQuestionsTask.Result,
                ActiveConnection = activeConnectionTask.Result
            };

            _logger.LogInformation("Analytics aggregation completed for job {JobId}", jobId);
            return Result.Success(response);
        }

        public async Task<Result<IEnumerable<RecentJobsSummaryResponse>>> GetRecentJobsAsync(string employerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching recent 5 jobs for employer {EmployerId}", employerId);

            var recentJobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => new RecentJobsSummaryResponse
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    Governorate = j.Governorate!.NameArabic,
                    WorkingDaysPerWeek = j.WorkingDaysPerWeek,
                    WorkingHoursPerDay = j.WorkingHoursPerDay,
                    NumberOfQuestions = j.Questions!.Count,
                    NumberOfApplications = j.Applications!.Count,
                    JobStatus = j.Status.ToString(),
                    CreatedAt = j.CreatedAt
                })
                .ToListAsync(cancellationToken);
                

            _logger.LogInformation("Retrieved {Count} recent jobs for employer {EmployerId}", recentJobs.Count, employerId);
            return Result.Success<IEnumerable<RecentJobsSummaryResponse>>(recentJobs);
        }
    }
}
