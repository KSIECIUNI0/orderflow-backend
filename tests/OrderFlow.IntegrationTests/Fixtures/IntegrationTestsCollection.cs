namespace OrderFlow.IntegrationTests.Fixtures;

[CollectionDefinition("Integration tests collection")]
public class IntegrationTestsCollection :
    ICollectionFixture<PostgreSqlContainerFixture>, 
    ICollectionFixture<LocalStackContainerFixture>
{
}