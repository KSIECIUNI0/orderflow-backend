using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.CollectionFixtures;

[CollectionDefinition("S3 Container Collection")]
public class S3ContainerCollection : ICollectionFixture<S3ContainerFixture>
{
}