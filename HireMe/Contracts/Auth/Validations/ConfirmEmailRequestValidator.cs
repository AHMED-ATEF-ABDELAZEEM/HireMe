using FluentValidation;
using HireMe.Contracts.Auth.Requests;

namespace HireMe.Contracts.Auth.Validations
{
    public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
    {
        public ConfirmEmailRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty();

        }
    }



}
