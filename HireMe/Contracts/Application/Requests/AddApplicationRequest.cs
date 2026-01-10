namespace HireMe.Contracts.Application.Requests
{
    public class AddApplicationRequest
    {
        public int JobId { get; set; }
        public string? Message { get; set; }
    }
}
