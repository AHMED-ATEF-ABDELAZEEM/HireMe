using System.ComponentModel.DataAnnotations.Schema;

namespace HireMe.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        [ForeignKey(nameof(Job))]
        public int JobId { get; set; }

        public Job? Job { get; set; }

        [ForeignKey(nameof(Worker))]
        public string WorkerId { get; set; } = string.Empty;

        public ApplicationUser? Worker { get; set; }
    }
}
