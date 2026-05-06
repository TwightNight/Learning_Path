using BussinessObject;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("--- LAZY LOADING ---");



using var context = new DBContext();

// SQL Statement 1: Only SELECT Categories
var categories = await context.Categories.ToListAsync();

foreach (var cat in categories)

{
    Console.WriteLine($"Category: {cat.Name}");


    // At this point, EF Core automatically fires a second (and third, fourth...) SQL statement
    // to SELECT Products with the corresponding CategoryId.
    // This is called the N+1 Query problem!
    foreach (var prod in cat.Products)

    {
        Console.WriteLine($" - Product: {prod.Name}");

    }

}