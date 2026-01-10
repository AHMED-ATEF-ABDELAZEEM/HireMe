namespace HireMe.Contracts.Answer.Requests
{
    public class AddAnswerRequest
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
