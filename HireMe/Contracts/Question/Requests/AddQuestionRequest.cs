namespace HireMe.Contracts.Question.Requests
{
    public class AddQuestionRequest
    {
        public int JobId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }
}
