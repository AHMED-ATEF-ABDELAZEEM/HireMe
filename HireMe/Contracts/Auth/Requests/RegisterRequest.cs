using HireMe.Enums;

namespace HireMe.Contracts.Auth.Requests
{
    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserType UserType { get; set; }
        public string Password { get; set; }
    }

}
