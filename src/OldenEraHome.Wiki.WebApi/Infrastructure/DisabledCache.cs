using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Models;

namespace OldenEraHome.Wiki.WebApi.Infrastructure;

public sealed class DisabledCache() : ICacheStorage
{
    public Task<Creature[]?> GetCreaturesAsync(string locale)
    {
        return Task.FromResult<Creature[]?>(null);
    }

    public Task SaveCreaturesAsync(string locale, IEnumerable<Creature> allCreatures)
    {
        return Task.CompletedTask;
    }
}
