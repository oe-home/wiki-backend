using System.IO.Abstractions;
using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Endpoints;
using OldenEraHome.Wiki.WebApi.Infrastructure;
using OldenEraHome.Wiki.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddSingleton<WikiInfoProvider>();
services.AddSingleton<ICacheStorage, DisabledCache>();
services.AddSingleton<IPersistentDataStorage>((sp) =>
    new PersistentDataStorage(
        new FileSystem(),
        Path.Combine(AppContext.BaseDirectory, "data")
    )
);

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
