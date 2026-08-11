using Microsoft.EntityFrameworkCore;
using MiniDevTo.Entities;

namespace MiniDevTo.Features.Author.Articles.CreateArticle;

public static class Data
{
    //get author name by author id
    internal static async Task<string> GetAuthorNameByIdAsync(int authorId, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors
            .Where(a => a.Id == authorId)
            .Select(a => a.FullName)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return author ?? string.Empty;
    }
    internal static async Task<Article> CreateArticleAsync(Article article, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (article == null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        //check author exists
        var authorExists = await dbContext.Authors.AnyAsync(a => a.Id == article.AuthorId, cancellationToken);
        if (!authorExists)
        {
            throw new InvalidOperationException($"Author with ID {article.AuthorId} does not exist.");
        }


        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        return article;
    }
}
