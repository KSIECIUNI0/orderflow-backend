using OrderFlow.TestCore.Fixtures;

namespace OrderFlow.Api.IntegrationTests.CollectionFixtures;

[CollectionDefinition("Store Containers Tests Collection")]
public class StoreContainersTestsCollection : 
    ICollectionFixture<S3ContainerFixture>,
    ICollectionFixture<SqsContainerFixture>
{
}