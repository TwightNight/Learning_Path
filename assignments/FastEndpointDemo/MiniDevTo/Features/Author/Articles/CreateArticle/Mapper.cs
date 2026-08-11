using MiniDevTo.Entities;

namespace MiniDevTo.Features.Author.Articles.CreateArticle;

public class ArticleMapper : Mapper<Request, Response, Article>
{

    private readonly AppDbContext _dbContext;
    public ArticleMapper(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<Response> FromEntityAsync(Article article, CancellationToken cancellationToken = default)
    {
        return new Response
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            AuthorName = article.AuthorName,
            CreatedOn = article.CreatedOn
        };
    }

    //request to entity
    public override async Task<Article> ToEntityAsync(Request request, CancellationToken cancellationToken = default)
    {
        // 2. DÙNG Resolve<AppDbContext>() để lấy context mới nhất, không bị disposed
        var dbContext = Resolve<AppDbContext>();

        return new Article
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId,
            // Truyền dbContext vừa lấy được vào hàm Data
            AuthorName = await Data.GetAuthorNameByIdAsync(request.AuthorId, dbContext, cancellationToken) ?? "Unknown Author",
            CreatedOn = DateTime.UtcNow
        };
    }

}