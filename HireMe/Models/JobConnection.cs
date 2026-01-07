using System.ComponentModel.DataAnnotations.Schema;
using HireMe.Enums;

namespace HireMe.Models
{
    public class JobConnection : BaseEntity
    {

        public DateTime InteractionEndDate { get; set; } = DateTime.UtcNow.AddDays(10); // default 10 days from start and after it user (worker/employer) can chat,feedback,report each other
        public DateTime? CancelledAt { get; set; }
        public JobConnectionStatus Status { get; set; } = JobConnectionStatus.Active;

        [ForeignKey(nameof(Job))]
        public int JobId { get; set; }
        public Job? Job { get; set; }

        [ForeignKey(nameof(Worker))]
        public string WorkerId { get; set; } = string.Empty;
        public ApplicationUser? Worker { get; set; }

        [ForeignKey(nameof(Employer))]
        public string EmployerId { get; set; } = string.Empty;
        public ApplicationUser? Employer { get; set; }

    }
}
