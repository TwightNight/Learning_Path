namespace MiniDevTo.Features.Public.GetArticle;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public override void Configure()
    {
        Get("/public/articles/{id:int}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await Data.GetArticleByIdAsync(req, _dbContext, ct);
        if (response is null)
        {
            await Send.NotFoundAsync();
        }
        await Send.OkAsync(response, cancellation: ct);
    }
}