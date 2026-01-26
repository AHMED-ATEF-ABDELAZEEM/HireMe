using System.ComponentModel.DataAnnotations.Schema;

namespace HireMe.Models
{
    public class Feedback : BaseEntity
    {
        [ForeignKey(nameof(JobConnection))]
        public int JobConnectionId { get; set; }
        public JobConnection? JobConnection { get; set; }

        [ForeignKey(nameof(FromUser))]
        public string FromUserId { get; set; } = string.Empty;
        public ApplicationUser? FromUser { get; set; }

        [ForeignKey(nameof(ToUser))]
        public string ToUserId { get; set; } = string.Empty;
        public ApplicationUser? ToUser { get; set; }

        public int Rating { get; set; } // 1-5 stars
        public string? Message { get; set; }

        public bool IsVisible { get; set; } = false;
    }
}
