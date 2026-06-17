using OrderFlow.TestCore.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.CollectionFixtures;

[CollectionDefinition("WireMock collection")]
public class WireMockCollection : ICollectionFixture<WireMockFixture>
{
}