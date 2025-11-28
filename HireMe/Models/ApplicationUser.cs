using Microsoft.AspNetCore.Identity;

namespace HireMe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? ImageProfile { get; set; } = string.Empty;

        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
