using System.Linq;
using System.Threading.Tasks;
using OldenEraHome.Wiki.WebApi.Infrastructure;

namespace OldenEraHome.Wiki.UnitTests.Infrastructure
{
    public class PersistentDataStorageTests
    {
        [Fact]
        public async Task GetCreaturesAsync_DeserializesDataCorrectly()
        {
            // Arrange
            var storage = new PersistentDataStorage();

            // Act
            var creatures = await storage.GetCreaturesAsync("en");

            // Assert
            Assert.NotNull(creatures);
            Assert.NotEmpty(creatures);

            var griffin = creatures.SingleOrDefault(c => c.Level == 3);
            Assert.NotNull(griffin);
            
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
            Assert.Single(griffin.Abilities);
        }
    }
}
