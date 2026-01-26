
using Microsoft.AspNetCore.Identity;

namespace HireMe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ImageProfile { get; set; } = string.Empty;

        // Feedback Rating Fields
        public int TotalRatingSum { get; set; } = 0;
        public int TotalRatingsCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;

        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
