using OrderFlow.TestCore.Fixtures;

namespace OrderFlow.Api.IntegrationTests.CollectionFixtures;

[CollectionDefinition("DataBase and Queue Tests Collection")]
public class DbAndQueueTestsCollection :
    ICollectionFixture<PostgreSqlContainerFixture>, 
    ICollectionFixture<SqsContainerFixture>
{
}