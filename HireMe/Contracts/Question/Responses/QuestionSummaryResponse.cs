namespace HireMe.Contracts.Question.Responses
{
    public class QuestionSummaryResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool HasAnswer { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUpdated { get; set; }
    }
}
