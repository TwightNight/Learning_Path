using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.CreatePost;

public class CreatePostValidator: Validator<CreatePostRequest>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");
    }
}