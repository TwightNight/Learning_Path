using BussinessObject;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("------EAGER LOADING-----");

using var context = new DBContext();
Console.WriteLine($"Database path: {context.DbPath}.");
// The generated SQL query will use JOIN.
var catergory = await context.Categories
    .Include(c => c.Products)
    .ToListAsync();

foreach(var c in catergory)
{
    Console.WriteLine($"Category: {c.Name}");

    foreach(var product in c.Products)
    {
        Console.WriteLine($" - Product: {product.Name}");
    }
}
