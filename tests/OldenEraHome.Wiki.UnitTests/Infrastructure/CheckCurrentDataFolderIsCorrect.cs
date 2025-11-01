using System.IO.Abstractions;
using OldenEraHome.Wiki.WebApi.Infrastructure;

namespace OldenEraHome.Wiki.UnitTests.Infrastructure;

public sealed class CheckCurrentDataFolderIsCorrect
{
    [Fact]
    public async Task BuildCurrentDataFolderNotThrowExceptions()
    {
        // Arrange
        var dataStorage = new PersistentDataStorage(
            new FileSystem(),
            Path.Combine(AppContext.BaseDirectory, "data")
        );

        // Act
        await dataStorage.InitAsync();

        // Assert
        // No exceptuons is success
    }
}
