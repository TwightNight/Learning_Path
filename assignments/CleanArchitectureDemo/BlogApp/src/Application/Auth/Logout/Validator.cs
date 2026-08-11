// BlogApp.Web/Endpoints/Auth/LogoutRequestValidator.cs
using BlogApp.Application.Auth.Logout;
using FastEndpoints;
using FluentValidation;

namespace BlogApp.Web.Endpoints.Auth;

public sealed class LogoutValidator : Validator<LogoutRequest>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("RefreshToken is required.");
    }
}