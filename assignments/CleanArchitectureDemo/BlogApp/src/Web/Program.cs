using BlogApp.Infrastructure.Data;
using BlogApp.Web.Middleware;
using FastEndpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseCors(static builder => 
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());
// app.UseHttpsRedirection();
app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.AddDocument("v1");
    options.OperationTitleSource = OperationTitleSource.Path;
});

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/scalar"));

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);
app.UseAuthentication()
    .UseMiddleware<TokenVersionMiddleware>()
   .UseAuthorization()
   .UseFastEndpoints();


app.Run();
