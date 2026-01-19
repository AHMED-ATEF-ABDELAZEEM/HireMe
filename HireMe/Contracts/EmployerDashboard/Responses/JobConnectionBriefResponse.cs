using HireMe.Contracts.Application.Responses;

namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class JobConnectionBriefResponse
    {
        public int JobConnectionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndsAt { get; set; }
        public WorkerInfoResponse Worker { get; set; } = null!;
    }
}
