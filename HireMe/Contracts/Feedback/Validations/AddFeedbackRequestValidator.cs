using FluentValidation;

namespace HireMe.Contracts.Feedback
{
    public class AddFeedbackRequestValidator : AbstractValidator<AddFeedbackRequest>
    {
        public AddFeedbackRequestValidator()
        {
            RuleFor(x => x.JobConnectionId)
                .GreaterThan(0)
                .WithMessage("Job connection ID must be greater than 0.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Message)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Message))
                .WithMessage("Feedback message cannot exceed 500 characters.");
        }
    }
}
