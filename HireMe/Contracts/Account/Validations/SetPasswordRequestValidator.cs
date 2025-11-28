using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Account.Requests;

namespace HireMe.Contracts.Account.Validations
{
    public class SetPasswordRequestValidator : AbstractValidator<SetPasswordRequest>
    {
        public SetPasswordRequestValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);

        }
    }
}
