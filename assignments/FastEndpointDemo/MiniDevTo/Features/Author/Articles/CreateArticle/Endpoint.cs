using MiniDevTo.Entities;

namespace MiniDevTo.Features.Author.Articles.CreateArticle;

//endpoint
public class Endpoint : Endpoint<Request, Response, ArticleMapper>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/author/articles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Article article = await Map.ToEntityAsync(req, ct);
        await Data.CreateArticleAsync(article, _dbContext, ct);
        var response = await Map.FromEntityAsync(article, ct);
        await Send.CreatedAtAsync<Public.GetArticle.Endpoint>(
            routeValues: new { id = response.Id }, 
            responseBody: response, 
            cancellation: ct);
    }
}