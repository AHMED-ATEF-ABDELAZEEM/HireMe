using FluentValidation;
using HireMe.Contracts.Answer.Requests;

namespace HireMe.Contracts.Answer.Validations
{
    public class AddAnswerRequestValidator : AbstractValidator<AddAnswerRequest>
    {
        public AddAnswerRequestValidator()
        {
            RuleFor(x => x.AnswerText)
                .NotEmpty()
                .WithMessage("Answer text is required.")
                .MaximumLength(1000)
                .WithMessage("Answer text must not exceed 1000 characters.");

            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("Valid question ID is required.");
        }
    }
}
