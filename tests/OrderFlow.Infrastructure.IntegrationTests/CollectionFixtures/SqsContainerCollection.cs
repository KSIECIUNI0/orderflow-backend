using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.CollectionFixtures;

[CollectionDefinition("SQS Container Collection")]
public class SqsContainerCollection : ICollectionFixture<SqsContainerFixture>
{
}