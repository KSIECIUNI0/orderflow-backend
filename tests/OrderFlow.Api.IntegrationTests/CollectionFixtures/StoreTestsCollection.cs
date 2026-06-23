using OrderFlow.TestCore.Fixtures;

namespace OrderFlow.Api.IntegrationTests.CollectionFixtures;

[CollectionDefinition("Store Tests Collection")]
public class StoreTestsCollection : ICollectionFixture<S3ContainerFixture>
{
}