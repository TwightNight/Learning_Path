global using FastEndpoints;
global using FluentValidation;
using FastEndpoints.OpenApi;
using Microsoft.EntityFrameworkCore;
using MiniDevTo.DbContext;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AppDb"));

builder.Services.AddFastEndpoints();
builder.Services.OpenApiDocument(o =>
{
    o.DocumentName = "v1";
    o.Title = "FastEndpoints OpenAPI Demo";
    o.Version = "v1";
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
    o =>
    {
        o.AddDocuments("v1"); //inform scalar of your doc names
        o.OperationTitleSource = OperationTitleSource.Path; //change title source
    });
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData.Seed(db);
    }
    app.Map("/", () => Results.Redirect("/scalar"));

}
app.UseDefaultExceptionHandler();
app.UseFastEndpoints();


app.Run();
