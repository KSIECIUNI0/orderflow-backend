using OrderFlow.TestCore.Fixtures;

namespace OrderFlow.Api.IntegrationTests.CollectionFixtures;

[CollectionDefinition("Orders Containers Tests Collection")]
public class OrdersContainersTestsCollection :
    ICollectionFixture<PostgreSqlContainerFixture>, 
    ICollectionFixture<SqsContainerFixture>
{
}