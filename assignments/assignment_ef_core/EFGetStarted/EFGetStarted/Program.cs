// See https://aka.ms/new-console-template for more information
using EFGetStarted;
using Microsoft.EntityFrameworkCore;

using var db = new BloggingContext();
// Note: This sample requires the database to be created before running.
Console.WriteLine($"Database path: {db.DbPath}.");

//Create
Console.WriteLine("Insert a new blog");
db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
await db.SaveChangesAsync();

//Read
Console.WriteLine("Query for a blog");
var blog = await db.Blogs
    .OrderBy(b => b.BlogId)
    .FirstAsync();
Console.WriteLine($"Blog - id: {blog.BlogId}, url: {blog.Url}");

//Update
Console.WriteLine("Update the blog and adding a post");
blog.Url = "https://devblogs.microsoft.com/dotnet";
blog.Posts.Add(
    new Post { Title = "Hello World", Content = "An app using EF Core" });
await db.SaveChangesAsync();

// Read After Updated
Console.WriteLine("Querying for a blog");
var blogsUpdated = await db.Blogs
    .OrderBy(b => b.BlogId)
    .ToListAsync();
Console.WriteLine($"Blog - id: {blog.BlogId}, url: {blog.Url}");

//Delete 
Console.WriteLine("Delete blog");
db.Remove(blog);
await db.SaveChangesAsync();
