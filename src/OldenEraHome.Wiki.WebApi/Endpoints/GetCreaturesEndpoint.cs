using Microsoft.AspNetCore.Mvc;
using OldenEraHome.Wiki.WebApi.Services;

namespace OldenEraHome.Wiki.WebApi.Endpoints;

public static class GetCreaturesEndpoint
{
    public static IServiceCollection AddGetCreaturesEndpointServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<EndpointServices>();
        return services;
    }

    public static IEndpointRouteBuilder MapGetCreaturesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/creatures", Handle);
        return app;
    }

    private static async Task<IResult> Handle(
        [AsParameters] EndpointRequest request,
        [FromServices] EndpointServices services
    )
    {
        var creatures = await services.InfoProvider.GetCreaturesAsync(request.Filter, request.Language);
        return Results.Ok(creatures);
    }

    private sealed record EndpointRequest(
        [FromQuery(Name = "filter")] string Filter = "",
        [FromQuery(Name = "lang")] string Language = "en"
    );

    private sealed record EndpointServices(
        WikiInfoProvider InfoProvider
    );

}

