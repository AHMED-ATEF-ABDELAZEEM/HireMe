namespace HireMe.Contracts.EmployerDashboard.Responses
{
    public class EmployerDashboardResponse
    {
        public List<PublishedJobCardResponse> PublishedJobs { get; set; } = new();
        public List<ActiveConnectionCardResponse> ActiveConnections { get; set; } = new();
    }
}
