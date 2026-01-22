namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class PublishedJobCardResponse
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int PendingApplications { get; set; }
        public int UnansweredQuestions { get; set; }
    }
}
