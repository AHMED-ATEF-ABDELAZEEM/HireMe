using FluentValidation;
using HireMe.Contracts.Auth.Requests;

namespace HireMe.Contracts.Auth.Validations
{
    public class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
    {
        public ResendConfirmationEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

        }
    }
}
