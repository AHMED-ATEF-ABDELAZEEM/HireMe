using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Auth.Requests;

namespace HireMe.Contracts.Auth.Validations
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Code)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);



        }
    }



}
