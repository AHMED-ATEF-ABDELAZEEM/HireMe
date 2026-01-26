namespace HireMe.Contracts.Feedback
{
    public record AddFeedbackRequest(
        int JobConnectionId,
        int Rating,
        string? Message
    );
}
