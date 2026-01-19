using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Storage.Monitoring;
using HireMe.Enums;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.BackgroundJobs
{

    public interface IApplicationStatusBackgroundJob
    {
        Task HandleApplicationAcceptanceAsync(int jobId, int applicationId, string workerId);
        Task HandleJobClosureAsync(int jobId);
    }
    public class ApplicationStatusBackgroundJob : IApplicationStatusBackgroundJob
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApplicationStatusBackgroundJob> _logger;

        public ApplicationStatusBackgroundJob(AppDbContext context, ILogger<ApplicationStatusBackgroundJob> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task HandleApplicationAcceptanceAsync(int jobId, int applicationId, string workerId)
        {
            try
            {
                var now = DateTime.UtcNow;
                await RejectOtherApplicationsForJobAsync(jobId, applicationId, now);
                await CloseWorkerApplicationsOnOtherJobsAsync(workerId, applicationId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling application acceptance for job {JobId} and application {ApplicationId}", jobId, applicationId);
                throw;
            }
        }

        private async Task RejectOtherApplicationsForJobAsync(int jobId,int acceptedApplicationId,DateTime now)
        {
            _logger.LogInformation("Rejecting other applications for job {JobId} after acceptance of application {ApplicationId}", jobId, acceptedApplicationId);
            await _context.Applications
                .Where(a => a.JobId == jobId &&
                            a.Id != acceptedApplicationId &&
                            a.Status == ApplicationStatus.Applied)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, ApplicationStatus.EmployerChooseAnotherWorker)
                    .SetProperty(a => a.StatusChangedAt, now));
            _logger.LogInformation("Successfully rejected other applications for job {JobId}", jobId);
        }

        private async Task CloseWorkerApplicationsOnOtherJobsAsync(string workerId,int acceptedApplicationId, DateTime now)
        {
            _logger.LogInformation("Closing other applications for worker {WorkerId} after acceptance of application {ApplicationId}", workerId, acceptedApplicationId);
            await _context.Applications
                .Where(a => a.WorkerId == workerId &&
                            a.Id != acceptedApplicationId &&
                            a.Status == ApplicationStatus.Applied)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, ApplicationStatus.WorkerAcceptedAtAnotherJob)
                    .SetProperty(a => a.StatusChangedAt, now));
            _logger.LogInformation("Successfully closed other applications for worker {WorkerId}", workerId);
        }

        public async Task HandleJobClosureAsync(int jobId)
        {
            try
            {
                _logger.LogInformation("Updating applications status to JobClosed for job {JobId}", jobId);
                
                var now = DateTime.UtcNow;
                var updatedCount = await _context.Applications
                    .Where(a => a.JobId == jobId && a.Status == ApplicationStatus.Applied)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Status, ApplicationStatus.JobClosed)
                        .SetProperty(a => a.StatusChangedAt, now));
                
                _logger.LogInformation("Successfully updated {Count} applications to JobClosed status for job {JobId}", updatedCount, jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling job closure for job {JobId}", jobId);
                throw;
            }
        }


    }
}
