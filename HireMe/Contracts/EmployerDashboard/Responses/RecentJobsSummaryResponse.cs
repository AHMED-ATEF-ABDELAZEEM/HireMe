namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class RecentJobsSummaryResponse
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public int WorkingDaysPerWeek { get; set; }
        public int WorkingHoursPerDay { get; set; }
        public int NumberOfQuestions { get; set; }
        public int NumberOfApplications { get; set; }
        /// <summary>
        /// Job status: Published, InProgress, Completed, Closed, Cancelled
        /// </summary>
        public string JobStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
