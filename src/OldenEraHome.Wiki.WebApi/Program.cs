using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Endpoints;
using OldenEraHome.Wiki.WebApi.Infrastructure;
using OldenEraHome.Wiki.WebApi.Servises;

var builder = WebApplication.CreateBuilder(args);
var servises = builder.Services;
var configuration = builder.Configuration;

servises.AddSingleton<WikiInfoProvider>();
servises.AddSingleton<ICacheStorage, DisabledCache>();
servises.AddSingleton<IPersistentDataStorage, PersistentDataStorage>();

servises.AddOpenApi();
servises.AddGetCreaturesEndpointServives(configuration);

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
