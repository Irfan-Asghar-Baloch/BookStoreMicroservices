using AuthenticationService.DTO;
using FluentValidation;

namespace AuthenticationService.Validators
{
    public class RegisterSignupRequestValidator : AbstractValidator<RegisterSignupRequest>
    {
        public RegisterSignupRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x=> x.Password).NotEmpty().MinimumLength(6);
        }
    }
}
