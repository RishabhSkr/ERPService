using FluentValidation;
using MyERP.Services.Identity.DTOs.Users;

namespace MyERP.Services.Identity.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username Required")
                .Length(3, 50)
                .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("Username special chars allow nahi karta (sirf . aur _)");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Password weak hai. Min 8 chars.")
                .Matches(@"[A-Z]").WithMessage("1 Uppercase letter chahiye")
                .Matches(@"[0-9]").WithMessage("1 Number chahiye")
                .Matches(@"[@$!%*?&#]").WithMessage("1 Special Char chahiye");

        }
    }
}