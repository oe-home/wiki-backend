using Microsoft.AspNetCore.Mvc;
using OldenEraHome.Wiki.WebApi.Servises;

namespace OldenEraHome.Wiki.WebApi.Endpoints;

public static class GetCreaturesEndpoint
{
    public static IServiceCollection AddGetCreaturesEndpointServives(
        this IServiceCollection servises,
        IConfiguration configuration
    )
    {
        servises.AddScoped<EndpointServises>();
        return servises;
    }

    public static IEndpointRouteBuilder MapGetCreaturesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/creatures", Handle);
        return app;
    }

    private static async Task<IResult> Handle(
        [AsParameters] EndpointRequest request,
        [FromServices] EndpointServises servises
    )
    {
        var creatures = await servises.InfoProvider.GetCreaturesAsync(request.Filter, request.Language);
        return Results.Ok(creatures);
    }

    private sealed record EndpointRequest(
        [FromQuery(Name = "filter")] string Filter = "",
        [FromQuery(Name = "lang")] string Language = "en"
    );

    private sealed record EndpointServises(
        WikiInfoProvider InfoProvider
    );

}

