using Hangfire.Common;
using HireMe.Contracts.WorkerDashboard;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services;

public class WorkerDashboardService : IWorkerDashboardService
{
    private readonly AppDbContext _context;

    public WorkerDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WorkerDashboardResponse>> GetWorkerDashboardAsync(string workerId, CancellationToken cancellationToken = default)
    {

        var activeConnection= await GetActiveJobConnectionAsync(workerId, cancellationToken);

        if (activeConnection != null && activeConnection.Status == JobConnectionStatus.Active.ToString())
        {
            return Result.Success(new WorkerDashboardResponse(
                Applications: null,
                Questions: null,
                ActiveConnection: activeConnection
            ));
        }

        // // First batch: Execute independent queries in parallel
        // var latestJobConnectionTask = GetLatestJobConnectionDateAsync(workerId, cancellationToken);
        // var activeConnectionTask = GetActiveJobConnectionAsync(workerId, cancellationToken);

        // await Task.WhenAll(latestJobConnectionTask, activeConnectionTask);

        // var latestJobConnectionDate = await latestJobConnectionTask;
        // var activeConnection = await activeConnectionTask;

        var latestJobConnectionDate = await GetLatestJobConnectionDateAsync(workerId, cancellationToken);

        // Second batch: Execute queries that depend on latestJobConnectionDate in parallel
        var applicationStatisticsTask = GetApplicationStatisticsAsync(workerId, latestJobConnectionDate, cancellationToken);
        var questionStatisticsTask = GetQuestionStatisticsAsync(workerId, latestJobConnectionDate, cancellationToken);

        await Task.WhenAll(applicationStatisticsTask, questionStatisticsTask);

        var applicationStatistics = await applicationStatisticsTask;
        var questionStatistics = await questionStatisticsTask;

        var response = new WorkerDashboardResponse(
            Applications: applicationStatistics,
            Questions: questionStatistics,
            ActiveConnection: activeConnection
        );

        return Result.Success(response);
    }

    private async Task<DateTime?> GetLatestJobConnectionDateAsync(string workerId, CancellationToken cancellationToken)
    {
        return await _context.JobConnections
            .Where(jc => jc.WorkerId == workerId)
            .OrderByDescending(jc => jc.CreatedAt)
            .Select(jc => jc.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ApplicationStatistics> GetApplicationStatisticsAsync(string workerId, DateTime? latestJobConnectionDate, CancellationToken cancellationToken)
    {
        // Filter applications: if worker has a JobConnection, only count applications AFTER the latest connection
        // This separates the current hiring process from historical data
        var applicationsQuery = _context.Applications
            .Where(a => a.WorkerId == workerId);

        if (latestJobConnectionDate.HasValue)
        {
            // Worker has at least one JobConnection - only analyze applications after that date
            applicationsQuery = applicationsQuery.Where(a => a.CreatedAt > latestJobConnectionDate.Value);
        }

        // Calculate statistics in the database using GroupBy
        var statusGroups = await applicationsQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Extract counts for each status
        var pending = statusGroups.FirstOrDefault(g => g.Status == ApplicationStatus.Applied)?.Count ?? 0;
        var rejected = statusGroups.FirstOrDefault(g => g.Status == ApplicationStatus.Rejected)?.Count ?? 0;
        var closed = statusGroups.FirstOrDefault(g => g.Status == ApplicationStatus.JobClosed)?.Count ?? 0;
        var chooseAnother = statusGroups.FirstOrDefault(g => g.Status == ApplicationStatus.EmployerChooseAnotherWorker)?.Count ?? 0;
        var withdrawn = statusGroups.FirstOrDefault(g => g.Status == ApplicationStatus.Withdrawn)?.Count ?? 0;
        var total = statusGroups.Sum(g => g.Count);

        return new ApplicationStatistics(
            Total: total,
            Pending: pending,
            Rejected: rejected,
            Closed: closed,
            ChooseAnotherPerson: chooseAnother,
            Withdrawn: withdrawn
        );
    }

    private async Task<QuestionStatistics> GetQuestionStatisticsAsync(string workerId, DateTime? latestJobConnectionDate, CancellationToken cancellationToken)
    {
        // Get both answered and unanswered question counts in a single query using conditional aggregation
        // Filter by datetime: if worker has JobConnection, only count questions AFTER latest connection
        // Only count questions for jobs that are still Published
        var questionsQuery = _context.Questions
            .Where(q => q.WorkerId == workerId && q.Job.Status == JobStatus.Published);

        if (latestJobConnectionDate.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.CreatedAt > latestJobConnectionDate.Value);
        }

        // Use GroupBy with conditional counting to get both statistics in one query
        var questionStats = await questionsQuery
            .GroupBy(q => 1)
            .Select(g => new
            {
                Answered = g.Count(q => q.Answer != null),
                Unanswered = g.Count(q => q.Answer == null)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new QuestionStatistics(
            AnsweredQuestions: questionStats?.Answered ?? 0,
            UnansweredQuestions: questionStats?.Unanswered ?? 0
        );
    }

    private async Task<ActiveJobConnectionResponse?> GetActiveJobConnectionAsync(string workerId, CancellationToken cancellationToken)
    {
        // Get active job connection if exists (not cancelled, not completed, and still within interaction period)
        return await _context.JobConnections
            .Where(jc => jc.WorkerId == workerId 
                && jc.InteractionEndDate > DateTime.UtcNow)
            .OrderByDescending(jc => jc.CreatedAt)
            .Select(jc => new ActiveJobConnectionResponse(
                jc.Id,
                jc.Job.JobTitle,
                jc.Employer.FirstName + " " + jc.Employer.LastName,
                jc.Employer.ImageProfile,
                jc.InteractionEndDate,
                (int)(jc.InteractionEndDate - DateTime.UtcNow).TotalDays,
                jc.Status.ToString()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
