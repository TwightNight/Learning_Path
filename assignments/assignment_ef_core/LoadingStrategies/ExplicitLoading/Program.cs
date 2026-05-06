using BussinessObject;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("--- EXPLICIT LOADING ---");

using var context = new DBContext();

// SQL Statement 1: Get a specific Category
var category = await context.Categories
    .FirstOrDefaultAsync(c => c.Id == 1);

Console.WriteLine($"Category: {category.Name}");

// At this point, category.Products is NULL.
// Actively command to load Products belonging to this Category using Load()
context.Entry(category)

.Collection(c => c.Products)

.Load(); // SQL Statement 2: Load now

// You can combine filtering sub-data before loading
//context.Entry(category).Collection(c => c.Products).Query().Where(p => p.Price > 100).Load();

foreach (var prod in category.Products)
{
    Console.WriteLine($" - Product: {prod.Name}");
}