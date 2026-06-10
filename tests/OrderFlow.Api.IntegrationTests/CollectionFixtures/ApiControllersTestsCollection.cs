using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Api.IntegrationTests.CollectionFixtures;

[CollectionDefinition("Api Controllers Tests Collection")]
public class ApiControllersTestsCollection :
    ICollectionFixture<PostgreSqlContainerFixture>, 
    ICollectionFixture<SqsContainerFixture>
{
}