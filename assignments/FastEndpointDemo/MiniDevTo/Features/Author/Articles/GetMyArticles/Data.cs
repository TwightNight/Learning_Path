using Microsoft.EntityFrameworkCore;

namespace MiniDevTo.Features.Author.Articles.GetMyArticles;

//data
public static class Data
{
    internal static async Task<List<Response>> GetMyArticlesAsync
    (Request request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        return await dbContext.Articles
            .Where(a => a.AuthorId == request.AuthorId)
            .Select(a => new Response
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                CreatedOn = a.CreatedOn
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
    }
}