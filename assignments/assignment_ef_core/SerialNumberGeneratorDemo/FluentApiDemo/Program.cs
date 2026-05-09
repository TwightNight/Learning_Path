using FluentApiDemo;

using var context = new FluentDbContext();

var product = new ProductFluent { Name = "iPhone 15" };

context.Products.Add(product);

context.SaveChanges();

Console.WriteLine($"Name: {product.Name}");

Console.WriteLine($"Serial: {product.SerialNumber}"); // Runs instantly, automatically retrieves data from the database