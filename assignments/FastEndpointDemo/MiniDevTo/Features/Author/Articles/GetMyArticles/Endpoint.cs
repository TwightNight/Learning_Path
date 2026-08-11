namespace MiniDevTo.Features.Author.Articles.GetMyArticles;

public sealed class Endpoint : Endpoint<Request, List<Response>>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public override void Configure()
    {
        Get("/author/articles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await Data.GetMyArticlesAsync(req, _dbContext, ct);
        await Send.OkAsync(response, cancellation: ct);
    }
}