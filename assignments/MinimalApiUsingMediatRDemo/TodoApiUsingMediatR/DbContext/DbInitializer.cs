using Microsoft.EntityFrameworkCore;


namespace TodoApiUsingMediatR.DbContext;
public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Todos.AnyAsync())
        {
            return;
        }

        var todos = new[]
        {
            new Todo
            {
                Name = "Learn Minimal API",
                IsComplete = false
            },
            new Todo
            {
                Name = "Learn EF Core",
                IsComplete = true
            },
            new Todo
            {
                Name = "Learn MediatR",
                IsComplete = false
            }
        };

        context.Todos.AddRange(todos);

        await context.SaveChangesAsync();
    }
}