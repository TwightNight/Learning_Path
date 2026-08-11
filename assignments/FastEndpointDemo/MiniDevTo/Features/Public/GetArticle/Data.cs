using Microsoft.EntityFrameworkCore;

namespace MiniDevTo.Features.Public.GetArticle;

public static class Data
{
    internal static async Task<Response> GetArticleByIdAsync
    (Request request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        return await dbContext.Articles
            .Where(a => a.Id == request.Id)
            .Select(a => new Response
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorName = a.Author.FullName,
                CreatedOn = a.CreatedOn
            })
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        
    }
}