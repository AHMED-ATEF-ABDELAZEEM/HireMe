using System.ComponentModel.DataAnnotations.Schema;

namespace HireMe.Models
{
    public class Answer : BaseEntity
    {

        public string AnswerText { get; set; } = string.Empty;

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }

        public Question? Question { get; set; }

        [ForeignKey(nameof(Employer))]
        public string EmployerId { get; set; } = string.Empty;

        public ApplicationUser? Employer { get; set; }
    }
}
