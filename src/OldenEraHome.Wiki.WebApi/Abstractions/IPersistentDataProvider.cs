using OldenEraHome.Wiki.WebApi.Models;

namespace OldenEraHome.Wiki.WebApi.Abstractions;

public interface IPersistentDataStorage
{
    Task<Creature[]> GetCreaturesAsync(string locale);
}
