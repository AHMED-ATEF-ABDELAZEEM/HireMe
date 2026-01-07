using System.ComponentModel.DataAnnotations.Schema;
using HireMe.Enums;

namespace HireMe.Models
{
    public class Application : BaseEntity
    {
        public string? Message { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;


        [ForeignKey(nameof(Job))]
        public int JobId { get; set; }

        public Job? Job { get; set; }

        [ForeignKey(nameof(Worker))]
        public string WorkerId { get; set; } = string.Empty;
        public ApplicationUser? Worker { get; set; }

    }
}
