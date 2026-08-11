using Microsoft.EntityFrameworkCore;

namespace MiniDevTo.Features.Public.GetArticlesList;

public static class Data
{
    internal static async Task<List<Response>> GetArticlesListAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        return await dbContext.Articles
            .Select(a => new Response
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                AuthorName = a.Author.FullName,
                CreatedOn = a.CreatedOn
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}