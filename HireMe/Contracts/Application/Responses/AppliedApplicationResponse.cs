namespace HireMe.Contracts.Application.Responses
{
    public class AppliedApplicationResponse
    {
        public int ApplicationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsUpdated { get; set; }
        public WorkerInfoResponse Worker { get; set; } = null!;
    }

    public class WorkerInfoResponse
    {
        public string WorkerId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ImageProfile { get; set; }
    }
}
