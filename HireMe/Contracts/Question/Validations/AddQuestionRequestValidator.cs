using FluentValidation;
using HireMe.Contracts.Question.Requests;

namespace HireMe.Contracts.Question.Validations
{
    public class AddQuestionRequestValidator : AbstractValidator<AddQuestionRequest>
    {
        public AddQuestionRequestValidator()
        {
            RuleFor(x => x.QuestionText)
                .NotEmpty()
                .WithMessage("Question text is required.")
                .MaximumLength(500)
                .WithMessage("Question text must not exceed 500 characters.");

        }
    }
}
