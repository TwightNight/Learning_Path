using FastEndpoints;

using FluentValidation;

namespace BlogApp.Application.Auth.Login;

public class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()

    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be blank.")
            .EmailAddress().WithMessage("Email is not in the correct format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be blank.")
            .MinimumLength(3).WithMessage("Password must contain at least 3 characters.");

    }
}