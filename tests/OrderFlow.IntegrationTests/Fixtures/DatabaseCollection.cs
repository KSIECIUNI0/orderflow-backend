namespace OrderFlow.IntegrationTests.Fixtures;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture> { }