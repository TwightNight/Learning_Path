using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.UpdatePost;

public class UpdatePostValidator : Validator<UpdatePostRequest>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Post id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");
    }
}