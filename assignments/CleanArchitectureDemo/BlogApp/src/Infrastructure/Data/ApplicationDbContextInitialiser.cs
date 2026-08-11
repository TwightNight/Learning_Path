using BlogApp.Domain.Constants;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogApp.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // 1. Kiểm tra nếu đã có dữ liệu User thì không seed nữa
        if (await _context.Users.AnyAsync())
        {
            return;
        }

        // 2. Tạo Users mẫu (1 Admin, 1 Author)
        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "System",
            UserName = "admin",
            Email = "admin@blogapp.com",
            PasswordHash = "123", // Chuỗi hash giả lập (hoặc hash từ PasswordHasher)
            Role = "Admin"
        };

        var authorUser = new User
        {
            FirstName = "John",
            LastName = "Doe",
            UserName = "johndoe",
            Email = "john.doe@example.com",
            PasswordHash = "123",
            Role = "Author"
        };

        _context.Users.AddRange(adminUser, authorUser);
        await _context.SaveChangesAsync(); // Lưu để EF Core tự sinh ID cho User

        // 3. Tạo Posts mẫu (Gán trực tiếp Navigation Property 'Author')
        var post1 = new Post
        {
            Title = "Chào mừng bạn đến với BlogApp!",
            Content = "Đây là bài viết đầu tiên trên hệ thống BlogApp được xây dựng bằng FastEndpoints và Entity Framework Core.",
            Author = adminUser,
            IsPublished = true,
            PublishedDate = DateTime.UtcNow.AddDays(-5)
        };

        var post2 = new Post
        {
            Title = "Hướng dẫn lập trình C# với FastEndpoints",
            Content = "FastEndpoints là một framework tuyệt vời giúp xây dựng REST API trong .NET gọn gàng theo mô hình REPR (Request-Endpoint-Response).",
            Author = authorUser,
            IsPublished = true,
            PublishedDate = DateTime.UtcNow.AddDays(-2)
        };

        var post3 = new Post
        {
            Title = "Bài viết nháp (Chưa xuất bản)",
            Content = "Nội dung bài viết này đang trong quá trình biên soạn...",
            Author = authorUser,
            IsPublished = false,
            PublishedDate = null
        };

        _context.Posts.AddRange(post1, post2, post3);
        await _context.SaveChangesAsync(); // Lưu để EF Core tự sinh ID cho Post

        // 4. Tạo Comments mẫu
        var comment1 = new Comment
        {
            Content = "Bài viết mở đầu rất tuyệt vời!",
            Post = post1,
            User = authorUser
        };

        var comment2 = new Comment
        {
            Content = "FastEndpoints thực sự rất nhanh và dễ viết!",
            Post = post2,
            User = adminUser
        };

        var comment3 = new Comment
        {
            Content = "Cảm ơn bạn đã chia sẻ kiến thức.",
            Post = post2,
            User = authorUser
        };

        _context.Comments.AddRange(comment1, comment2, comment3);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Database seeded successfully with sample Users, Posts, and Comments.");
    }
}
