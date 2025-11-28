using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Account.Requests;

namespace HireMe.Contracts.Account.Validations
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.currentPassword)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);

            RuleFor(x => x.newPassword)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);

            RuleFor(x => x)
                .Must(x => x.newPassword != x.currentPassword)
                .WithMessage("New password must be different from the current password.");
        }
    }
}
