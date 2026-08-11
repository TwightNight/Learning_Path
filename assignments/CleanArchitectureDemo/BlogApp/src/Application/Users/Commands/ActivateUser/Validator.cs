// Application/Users/Commands/ActivateUser/ActivateUserValidator.cs
using FastEndpoints;

namespace BlogApp.Application.Users.Commands.ActivateUser;

public class ActivateUserValidator : Validator<ActivateUserRequest>
{
    public ActivateUserValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("User id is required.");
    }
}