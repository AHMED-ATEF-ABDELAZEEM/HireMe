namespace HireMe.Contracts.Job.Responses
{
    public class JobSummaryResponse
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string GovernorateName { get; set; } = string.Empty;
        public int NumberOfQuestions { get; set; }
        public int NumberOfApplications { get; set; }
        public int WorkingHoursPerDay { get; set; }
        public int WorkingDaysPerWeek { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUpdated { get; set; }
    }
}
