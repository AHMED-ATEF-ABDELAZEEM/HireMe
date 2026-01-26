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
        Task<Result<EmployerDashboardResponse>> GetEmployerDashboardAsync(string employerId, CancellationToken cancellationToken = default);
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

            // Single query with projection and aggregation
            var analytics = await _context.Applications
                .AsNoTracking()
                .Where(a => a.JobId == jobId)
                .GroupBy(a => a.JobId)
                .Select(g => new
                {
                    TotalApplications = g.Count(),
                    AppliedCount = g.Count(a => a.Status == ApplicationStatus.Applied),
                    RejectedCount = g.Count(a => a.Status == ApplicationStatus.Rejected),
                    WithdrawnCount = g.Count(a => a.Status == ApplicationStatus.Withdrawn),
                    AcceptedAtAnotherJobCount = g.Count(a => a.Status == ApplicationStatus.WorkerAcceptedAtAnotherJob),
                    LastApplicationAt = g.Max(a => (DateTime?)a.CreatedAt)
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Separate query for unanswered questions
            var unansweredQuestions = await _context.Questions
                .AsNoTracking()
                .Where(q => q.JobId == jobId && q.Answer == null)
                .CountAsync(cancellationToken);

            var response = new JobAnalyticsResponse
            {
                JobId = jobId,
                JobStatus = job.Status.ToString(),
                NumberOfApplications = analytics?.TotalApplications ?? 0,
                LastApplicationAt = analytics?.LastApplicationAt,
                AppliedApplications = analytics?.AppliedCount ?? 0,
                RejectedApplications = analytics?.RejectedCount ?? 0,
                WithdrawnApplications = analytics?.WithdrawnCount ?? 0,
                AcceptedAtAnotherJobApplications = analytics?.AcceptedAtAnotherJobCount ?? 0,
                UnansweredQuestions = unansweredQuestions
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

        public async Task<Result<EmployerDashboardResponse>> GetEmployerDashboardAsync(string employerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching dashboard for employer {EmployerId}", employerId);

            // Get published jobs with pending applications and unanswered questions
            var publishedJobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.EmployerId == employerId && j.Status == JobStatus.Published)
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new PublishedJobCardResponse
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    PendingApplications = j.Applications!.Count(a => a.Status == ApplicationStatus.Applied),
                    UnansweredQuestions = j.Questions!.Count(q => q.Answer == null)
                })
                .ToListAsync(cancellationToken);

            // Get active connections where InteractionEndDate is in the future
            var now = DateTime.UtcNow;
            var activeConnections = await _context.JobConnections
                .AsNoTracking()
                .Include(jc => jc.Job)
                .Include(jc => jc.Worker)
                .Where(jc => jc.EmployerId == employerId && jc.InteractionEndDate > now)
                .OrderByDescending(jc => jc.CreatedAt)
                .Select(jc => new ActiveConnectionCardResponse
                {
                    Id = jc.Id,
                    JobTitle = jc.Job!.JobTitle,
                    Worker = new Contracts.Application.Responses.WorkerInfoResponse
                    {
                        WorkerId = jc.WorkerId,
                        FullName = jc.Worker!.FirstName + " " + jc.Worker.LastName,
                        ImageProfile = jc.Worker.ImageProfile
                    },
                    EndsAt = jc.InteractionEndDate
                })
                .ToListAsync(cancellationToken);

            var response = new EmployerDashboardResponse
            {
                PublishedJobs = publishedJobs,
                ActiveConnections = activeConnections
            };

            _logger.LogInformation("Retrieved {JobCount} published jobs and {ConnectionCount} active connections for employer {EmployerId}", 
                publishedJobs.Count, activeConnections.Count, employerId);

            return Result.Success(response);
        }
    }
}
