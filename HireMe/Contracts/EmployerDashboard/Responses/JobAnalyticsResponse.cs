namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class JobAnalyticsResponse
    {
        public int JobId { get; set; }
        public string JobStatus { get; set; } = null!;

        public int NumberOfApplications { get; set; }
        public int AppliedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int WithdrawnApplications { get; set; }
        public int AcceptedAtAnotherJobApplications { get; set; }

        public int UnansweredQuestions { get; set; }

        public DateTime? LastApplicationAt { get; set; }
    }
}
