namespace HireMe.Contracts.Answer.Responses
{
    public class AnswerSummaryResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUpdated { get; set; }
    }
}
