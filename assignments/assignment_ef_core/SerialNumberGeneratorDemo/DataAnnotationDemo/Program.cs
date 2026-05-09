using DataAnnotationDemo;

using var context = new AnnotationDbContext();

var product = new ProductAnnotation { Name = "Laptop Dell X" };
context.Products.Add(product);
context.SaveChanges();

Console.WriteLine($"Name: {product.Name}");
Console.WriteLine($"Serial: {product.SerialNumber}"); // Will print NULL if Migration is not manually modified