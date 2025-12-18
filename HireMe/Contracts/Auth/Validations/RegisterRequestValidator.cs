using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Auth.Requests;
using HireMe.Enums;

namespace HireMe.Contracts.Auth.Validations
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .Matches(PasswordRules.PasswordPattern)
                .WithMessage(PasswordRules.PasswordErrorMessage);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .Length(3, 100);
            RuleFor(x => x.LastName)
                .NotEmpty()
                .Length(3, 100);

            RuleFor(x => x.UserType)
                .NotEqual(UserType.UnDefined)
                .WithMessage("User type is required.")
                .IsInEnum()
                .WithMessage("Invalid user type.");


        }
    }
}
