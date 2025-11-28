using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class TokenError
    {
        public static Error InvalidToken => new Error("Token.Invalid", "Invalid Token or Expired");
    }
}
