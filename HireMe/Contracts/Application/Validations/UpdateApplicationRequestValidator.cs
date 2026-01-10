using FluentValidation;
using HireMe.Contracts.Application.Requests;

namespace HireMe.Contracts.Application.Validations
{
    public class UpdateApplicationRequestValidator : AbstractValidator<UpdateApplicationRequest>
    {
        public UpdateApplicationRequestValidator()
        {
            RuleFor(x => x.Message)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Message))
                .WithMessage("Message must not exceed 500 characters.");
        }
    }
}
