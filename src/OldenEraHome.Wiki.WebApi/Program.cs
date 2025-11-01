using System.IO.Abstractions;
using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Endpoints;
using OldenEraHome.Wiki.WebApi.Infrastructure;
using OldenEraHome.Wiki.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddSingleton<IFileSystem, FileSystem>();
services.AddSingleton<WikiInfoProvider>();
services.AddSingleton<ICacheStorage, DisabledCache>();
services.AddSingleton<IPersistentDataStorage, PersistentDataStorage>();

services.AddOpenApi();
services.AddGetCreaturesEndpointServices(configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGetCreaturesEndpoint();
app.MapGet("/health", () =>
{
    return "Healthy";
})
.WithName("GetHealthStatus");

app.Run();
