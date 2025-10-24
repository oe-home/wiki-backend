var builder = WebApplication.CreateBuilder(args);
var servises = builder.Services;
var confuguration = builder.Configuration;

servises.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
{
    return "Healthy";
})
.WithName("GetHealthStatus");

app.Run();
