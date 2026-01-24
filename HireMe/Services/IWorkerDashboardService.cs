using HireMe.Contracts.WorkerDashboard;
using HireMe.CustomResult;

namespace HireMe.Services;

public interface IWorkerDashboardService
{
    Task<Result<WorkerDashboardResponse>> GetWorkerDashboardAsync(string workerId, CancellationToken cancellationToken = default);
}
