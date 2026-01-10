using FluentValidation;
using HireMe.Contracts.Answer.Requests;

namespace HireMe.Contracts.Answer.Validations
{
    public class UpdateAnswerRequestValidator : AbstractValidator<UpdateAnswerRequest>
    {
        public UpdateAnswerRequestValidator()
        {
            RuleFor(x => x.AnswerText)
                .NotEmpty()
                .WithMessage("Answer text is required.")
                .MaximumLength(1000)
                .WithMessage("Answer text must not exceed 1000 characters.");
        }
    }
}
