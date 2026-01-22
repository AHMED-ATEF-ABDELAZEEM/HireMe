using HireMe.Contracts.Application.Responses;

namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class ActiveConnectionCardResponse
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public WorkerInfoResponse Worker { get; set; } = null!;
        public DateTime EndsAt { get; set; }
    }
}
