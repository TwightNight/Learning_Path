namespace MiniDevTo.Features.Author.Articles.CreateArticle;

//validator
public class Validator: Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(1000).WithMessage("Content must not exceed 1000 characters.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters long.");
        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("AuthorId must be greater than 0.");
    }
}