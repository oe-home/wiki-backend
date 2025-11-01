using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Models;

namespace OldenEraHome.Wiki.WebApi.Services;

public sealed class WikiInfoProvider(
    IPersistentDataStorage persistentDataStorage,
    ICacheStorage cache
)
{
    public async Task<List<Creature>> GetCreaturesAsync(string filter, string locale)
    {
        Creature[] allCreatures;

        var cahedData = await cache.GetCreaturesAsync(locale);
        if (cahedData is null)
        {
            allCreatures = await persistentDataStorage.GetCreaturesAsync(locale);
            _ = cache.SaveCreaturesAsync(locale, allCreatures);
        }
        else
        {
            allCreatures = cahedData;
        }

        return allCreatures
            .Where(c => c.Name.Contains(filter) || c.Fraction.Contains(filter))
            .ToList();
    }
}
