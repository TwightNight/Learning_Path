using MiniDevTo.Entities;

namespace MiniDevTo.DbContext;

public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        // Tránh seed nhiều lần
        if (db.Authors.Any())
            return;

        var author1 = new Author
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            UserName = "johndoe",
            Email = "john@example.com",
            Password = "123456",
            SignUpDate = new DateOnly(2026, 7, 20)
        };

        var author2 = new Author
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            UserName = "janesmith",
            Email = "jane@example.com",
            Password = "123456",
            SignUpDate = new DateOnly(2026, 7, 21)
        };

        db.Authors.AddRange(author1, author2);

        db.Articles.AddRange(
            new Article
            {
                Id = 1,
                AuthorId = 1,
                AuthorName = author1.FullName,
                Title = "Getting Started with FastEndpoints",
                Content = "This is my first article.",
                CreatedOn = DateTime.Now.AddDays(-2),
                IsApproved = true,
                Author = author1
            },
            new Article
            {
                Id = 2,
                AuthorId = 1,
                AuthorName = author1.FullName,
                Title = "Minimal API vs FastEndpoints",
                Content = "Let's compare them.",
                CreatedOn = DateTime.Now.AddDays(-1),
                IsApproved = true,
                Author = author1
            },
            new Article
            {
                Id = 3,
                AuthorId = 2,
                AuthorName = author2.FullName,
                Title = "EF Core InMemory Database",
                Content = "Learning EF Core.",
                CreatedOn = DateTime.Now,
                IsApproved = false,
                RejectionReason = "Need more details.",
                Author = author2
            });

        db.SaveChanges();
    }
}