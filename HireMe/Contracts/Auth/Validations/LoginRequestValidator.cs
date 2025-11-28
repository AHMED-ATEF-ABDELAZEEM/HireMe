using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Auth.Requests;

namespace HireMe.Contracts.Auth.Validations
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.password)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);

        }
    }
}
