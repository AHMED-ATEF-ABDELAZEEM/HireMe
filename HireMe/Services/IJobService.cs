using System.Numerics;
using Hangfire;
using HireMe.BackgroundJobs;
using HireMe.Consts;
using HireMe.Contracts.Job.Requests;
using HireMe.Contracts.Job.Responses;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IJobService
    {
        Task<Result<Job>> CreateJobAsync(string employerId, JobRequest jobRequest, CancellationToken cancellationToken = default);
        Task<Result<JobResponse>> GetJobByIdAsync(int jobId, CancellationToken cancellationToken = default);
        Task<Result> CloseJobAsync(int jobId, CancellationToken cancellationToken = default);
        Task<Result<int>> GetLastJobIdForEmployerAsync(string employerId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<JobSummaryResponse>>> GetAllJobsAsync(CancellationToken cancellationToken = default);
    }

    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobService> _logger;
        public JobService(AppDbContext context, ILogger<JobService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Job>> CreateJobAsync(string employerId, JobRequest jobRequest, CancellationToken cancellationToken = default)
        {

            _logger.LogInformation("starting job creation process");

            bool IsValidGovernorate = await  _context.Governorates.AnyAsync(g => g.Id == jobRequest.GovernorateId, cancellationToken);
            if (!IsValidGovernorate)
            {
                _logger.LogWarning("job creation failed: Invalid governorate ID: {GovernorateId}", jobRequest.GovernorateId);
                return Result.Failure<Job>(JobErrors.InvalidGovernorate);
            }

            var job = jobRequest.Adapt<Job>();

            job.EmployerId = employerId;
            job.WorkingDaysPerWeek = CalculateWorkingDayPerWeek(jobRequest.WorkDays);
            job.WorkingHoursPerDay = CalculateShiftDurationHours(jobRequest.ShiftStartTime, jobRequest.ShiftEndTime);
            job.ShiftType =   jobRequest.ShiftStartTime.Hour < 12 ? ShiftType.Morning : ShiftType.Night;



            await _context.Jobs.AddAsync(job, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("job created successfully with ID: {JobId}", job.Id);
            return Result.Success(job);
        }

        private int CalculateWorkingDayPerWeek(int WorkDays)
        {
            return BitOperations.PopCount((uint)WorkDays);
        }

        private int CalculateShiftDurationHours(TimeOnly shiftStartTime, TimeOnly shiftEndTime)
        {
            DateTime start = new DateTime(1, 1, 1, shiftStartTime.Hour, shiftStartTime.Minute, shiftStartTime.Second);
            DateTime end = new DateTime(1, 1, 1, shiftEndTime.Hour, shiftEndTime.Minute, shiftEndTime.Second);

            if (end <= start)
            {
                end = end.AddDays(1);
            }

            var hours = (end - start).TotalHours;

            return (int)hours;
        }

        public async Task<Result<JobResponse>> GetJobByIdAsync(int jobId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving job with ID: {JobId}", jobId);

            var job = await _context.Jobs
                .Where(j => j.Id == jobId)
                .Select(j => new JobResponse
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    Salary = j.Salary,
                    HasAccommodation = j.HasAccommodation,
                    WorkingDaysPerWeek = j.WorkingDaysPerWeek,
                    WorkingHoursPerDay = j.WorkingHoursPerDay,
                    Gender = j.Gender,
                    ShiftType = j.ShiftType,
                    ShiftStartTime = j.ShiftStartTime,
                    ShiftEndTime = j.ShiftEndTime,
                    Address = j.Address,
                    Description = j.Description,
                    Experience = j.Experience,
                    GovernorateName = j.Governorate.NameArabic,
                    WorkingDaysInArabic = WorkDaysInArabic.GetDays(j.WorkDays)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
            {
                _logger.LogWarning("Failed to retrieve job: Job with ID {JobId} not found", jobId);
                return Result.Failure<JobResponse>(JobErrors.JobNotFound);
            }

            _logger.LogInformation("Successfully retrieved job with ID: {JobId}", jobId);
            return Result.Success(job);
        }

        public async Task<Result> CloseJobAsync(int jobId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to close job with ID: {JobId}", jobId);

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

            if (job is null)
            {
                _logger.LogWarning("Failed to close job: Job with ID {JobId} not found", jobId);
                return Result.Failure(JobErrors.JobNotFound);
            }

            if (job.Status == JobStatus.Closed)
            {
                _logger.LogWarning("Failed to close job: Job with ID {JobId} is already closed", jobId);
                return Result.Failure(JobErrors.JobAlreadyClosed);
            }

            job.Status = JobStatus.Closed;
            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue background job to update all pending applications to JobClosed status
            BackgroundJob.Enqueue<IApplicationStatusBackgroundJob>(x => x.HandleJobClosureAsync(jobId));

            _logger.LogInformation("Successfully closed job with ID: {JobId} and enqueued background job to update applications", jobId);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<JobSummaryResponse>>> GetAllJobsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all jobs");

            var jobs = await _context.Jobs
                .Where(j => j.Status == JobStatus.Published)
                .Select(j => new JobSummaryResponse
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    Salary = j.Salary,
                    GovernorateName = j.Governorate.NameArabic,
                    NumberOfQuestions = j.Questions != null ? j.Questions.Count : 0,
                    NumberOfApplications = j.Applications != null ? j.Applications.Count : 0,
                    WorkingHoursPerDay = j.WorkingHoursPerDay,
                    WorkingDaysPerWeek = j.WorkingDaysPerWeek,
                    CreatedAt = j.CreatedAt,
                    IsUpdated = j.UpdatedAt != null
                })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved {JobCount} jobs", jobs.Count);
            return Result.Success<IEnumerable<JobSummaryResponse>>(jobs);
        }
        public async Task<Result<int>> GetLastJobIdForEmployerAsync(string employerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving last job ID for employer {EmployerId}", employerId);

            var lastJob = await _context.Jobs
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastJob is null)
            {
                _logger.LogWarning("No jobs found for employer {EmployerId}", employerId);
                return Result.Failure<int>(JobErrors.NoJobsForEmployer);
            }

            _logger.LogInformation("Successfully retrieved last job ID {JobId} for employer {EmployerId}", lastJob.Id, employerId);
            return Result.Success(lastJob.Id);
        }
    }
}