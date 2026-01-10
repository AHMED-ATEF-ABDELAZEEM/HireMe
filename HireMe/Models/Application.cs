using System.ComponentModel.DataAnnotations.Schema;
using HireMe.Enums;

namespace HireMe.Models
{
    public class Application : BaseEntity
    {
        public string? Message { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        public int JobId { get; set; }

        public Job? Job { get; set; }

        public string WorkerId { get; set; } = string.Empty;
        public ApplicationUser? Worker { get; set; }

    }
}
