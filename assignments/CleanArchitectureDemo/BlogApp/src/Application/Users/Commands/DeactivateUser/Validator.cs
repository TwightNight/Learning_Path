// Application/Users/Commands/DeactivateUser/DeactivateUserValidator.cs
using FastEndpoints;

namespace BlogApp.Application.Users.Commands.DeactivateUser;

public class DeactivateUserValidator : Validator<DeactivateUserRequest>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("User id is required.");
    }
}