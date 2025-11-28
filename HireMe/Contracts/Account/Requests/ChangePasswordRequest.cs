namespace HireMe.Contracts.Account.Requests
{
    public class ChangePasswordRequest
    {
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }
}
