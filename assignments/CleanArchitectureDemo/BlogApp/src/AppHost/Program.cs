using BlogApp.Shared;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddSqlite(Services.Database);

// if (builder.Environment.IsDevelopment())
// {
//     databaseServer.WithSqliteWeb();
// }
var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

var frontend = builder.AddViteApp(Services.WebFrontend, "../Web/ClientApp")
    .WithReference(web)
    .WaitFor(web)
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_URL", web.GetEndpoint("http"));

builder.Build().Run();
