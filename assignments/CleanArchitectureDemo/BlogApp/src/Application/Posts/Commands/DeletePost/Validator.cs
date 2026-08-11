using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.DeletePost;

public class DeletePostValidator : Validator<DeletePostRequest>
{
    public DeletePostValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Post id is required.");
    }
}