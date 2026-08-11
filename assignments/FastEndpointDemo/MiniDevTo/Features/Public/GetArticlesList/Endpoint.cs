using System.Collections.Generic;

namespace MiniDevTo.Features.Public.GetArticlesList;

public sealed class Endpoint: EndpointWithoutRequest<List<Response>>
{
    private readonly AppDbContext _dbContext;
    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    
    }
    
    public override void Configure()
    {
        Get("/public/articles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var responses = await Data.GetArticlesListAsync(_dbContext, ct);
        await Send.OkAsync(responses, cancellation: ct);
    }
}