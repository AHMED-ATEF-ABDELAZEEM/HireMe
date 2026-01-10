using FluentValidation;
using HireMe.Contracts.Application.Requests;

namespace HireMe.Contracts.Application.Validations
{
    public class AddApplicationRequestValidator : AbstractValidator<AddApplicationRequest>
    {
        public AddApplicationRequestValidator()
        {
            RuleFor(x => x.JobId)
                .GreaterThan(0)
                .WithMessage("Valid job ID is required.");

            RuleFor(x => x.Message)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Message))
                .WithMessage("Message must not exceed 500 characters.");
        }
    }
}
