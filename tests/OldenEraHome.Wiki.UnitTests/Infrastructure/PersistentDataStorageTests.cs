using System.IO.Abstractions.TestingHelpers;
using OldenEraHome.Wiki.WebApi.Infrastructure;

namespace OldenEraHome.Wiki.UnitTests.Infrastructure;

public class PersistentDataStorageTests
{
    [Fact]
    public async Task GetCreaturesAsync_DeserializesDataCorrectly()
    {
        // Arrange
        const string baseDir = "/data";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                $"{baseDir}/creatures.yml", new MockFileData(
                    """
                    - name: "$griffin_name"
                      level: 3
                      fraction: "$temple"
                      type: "$magic_creature"
                      health: 25
                      attack: 8
                      defence: 8
                      minDamage: 5
                      maxDamage: 9
                      initiative: 9
                      speed: 4
                      morale: 0
                      luck: 0
                      abilities:
                        - flying
                    """)
            },
            {
                $"{baseDir}/abilities.yml", new MockFileData(
                    """
                    flying:
                      name: "$flying_ability_name"
                      description: "$flying_ability_description"
                    """)
            },
            {
                $"{baseDir}/locale/en.yml", new MockFileData(
                    """
                    griffin_name: Griffin
                    temple: Temple
                    magic_creature: Magic Creature
                    flying_ability_name: Flying
                    flying_ability_description: Can fly over walls.
                    """)
            }
        });

        var storage = new PersistentDataStorage(mockFileSystem, baseDir);

        // Act
        var creatures = await storage.GetCreaturesAsync("en");

        // Assert
        Assert.NotNull(creatures);
        Assert.NotEmpty(creatures);

        var griffin = creatures.SingleOrDefault(c => c.Name == "Griffin");
        Assert.NotNull(griffin);

        Assert.Equal("Griffin", griffin.Name);
        Assert.Equal("Temple", griffin.Fraction);
        Assert.Equal("Magic Creature", griffin.Type);
        Assert.Equal(3u, griffin.Level);
        Assert.Equal(25u, griffin.Health);
        Assert.Equal(8u, griffin.Attack);
        Assert.Equal(8u, griffin.Defence);
        Assert.Equal(5u, griffin.MinDamage);
        Assert.Equal(9u, griffin.MaxDamage);
        Assert.Equal(9u, griffin.Initiative);
        Assert.Equal(4u, griffin.Speed);
        Assert.Equal(0, griffin.Morale);
        Assert.Equal(0, griffin.Luck);

        Assert.NotNull(griffin.Abilities);
        var ability = Assert.Single(griffin.Abilities);
        Assert.Equal("Flying", ability.Name);
        Assert.Equal("Can fly over walls.", ability.Description);
    }

    [Fact]
    public async Task GetCreaturesAsync_ThrowsWhenCreaturesFileIsMissing()
    {
        // Arrange
        const string baseDir = "/data";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            // creatures.yml is missing
            {
                $"{baseDir}/abilities.yml", new MockFileData(
                    """
                    flying:
                      name: "$flying_ability_name"
                      description: "$flying_ability_description"
                    """)
            },
            {
                $"{baseDir}/locale/en.yml", new MockFileData(
                    """
                    griffin_name: Griffin
                    temple: Temple
                    magic_creature: Magic Creature
                    flying_ability_name: Flying
                    flying_ability_description: Can fly over walls.
                    """)
            }
        });

        var storage = new PersistentDataStorage(mockFileSystem, baseDir);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => storage.GetCreaturesAsync("en"));
        Assert.Contains("Failed to load or deserialize file", exception.Message);
        Assert.Contains("creatures.yml", exception.Message);
    }

    [Fact]
    public async Task GetCreaturesAsync_ThrowsWhenLocalizationIsMissing()
    {
        // Arrange
        const string baseDir = "/data";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {
                $"{baseDir}/creatures.yml", new MockFileData(
                    """
                    - name: "$griffin_name"
                      level: 3
                      fraction: "$temple"
                      type: "$magic_creature"
                      health: 25
                      attack: 8
                      defence: 8
                      minDamage: 5
                      maxDamage: 9
                      initiative: 9
                      speed: 4
                      morale: 0
                      luck: 0
                      abilities:
                        - flying
                    """)
            },
            {
                $"{baseDir}/abilities.yml", new MockFileData(
                    """
                    flying:
                      name: "$flying_ability_name"
                      description: "$flying_ability_description"
                    """)
            },
            {
                $"{baseDir}/locale/en.yml", new MockFileData(
                    """
                    griffin_name: Griffin
                    # temple: Temple is missing
                    magic_creature: Magic Creature
                    flying_ability_name: Flying
                    flying_ability_description: Can fly over walls.
                    """)
            }
        });

        var storage = new PersistentDataStorage(mockFileSystem, baseDir);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidCastException>(() => storage.GetCreaturesAsync("en"));
        Assert.Contains("Can't be resolve localization for value: $temple", exception.Message);
    }
}
