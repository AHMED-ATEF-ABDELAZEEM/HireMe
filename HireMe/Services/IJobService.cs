using System.Numerics;
using HireMe.Contracts.Job.Requests;
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
        Task<Result<Job>> CreateJobAsync(JobRequest jobRequest,CancellationToken cancellationToken = default);
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

        public async Task<Result<Job>> CreateJobAsync(JobRequest jobRequest, CancellationToken cancellationToken = default)
        {

            _logger.LogInformation("starting job creation process");

            bool IsValidGovernorate = await  _context.Governorates.AnyAsync(g => g.Id == jobRequest.GovernorateId, cancellationToken);
            if (!IsValidGovernorate)
            {
                _logger.LogWarning("job creation failed: Invalid governorate ID: {GovernorateId}", jobRequest.GovernorateId);
                return Result.Failure<Job>(JobErrors.InvalidGovernorate);
            }

            var job = jobRequest.Adapt<Job>();

            job.WorkingDaysPerWeek = CalculateWorkingDayPerWeek(jobRequest.WorkDays);
            job.WorkingHoursPerDay = CalculateShiftDurationHours(jobRequest.ShiftStartTime, jobRequest.ShiftEndTime);
            job.ShiftType =   jobRequest.ShiftStartTime.Hour < 12 ? ShiftType.Morning : ShiftType.Night;



                // await _context.Jobs.AddAsync(job, cancellationToken);
                // await  _context.SaveChangesAsync(cancellationToken);
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




    }
}