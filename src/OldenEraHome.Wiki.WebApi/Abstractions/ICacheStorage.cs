using OldenEraHome.Wiki.WebApi.Models;

namespace OldenEraHome.Wiki.WebApi.Abstractions;

public interface ICacheStorage
{
    Task<Creature[]?> GetCreaturesAsync(string locale);
    Task SaveCreaturesAsync(string locale, IEnumerable<Creature> allCreatures);
}
