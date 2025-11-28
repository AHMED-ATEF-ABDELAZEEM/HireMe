using FluentValidation;
using HireMe.Contracts.Auth.Requests;

namespace HireMe.Contracts.Auth.Validations
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

        }
    }



}
