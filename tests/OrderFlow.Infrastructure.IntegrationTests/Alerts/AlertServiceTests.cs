using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Alerts.DTOs;
using OrderFlow.Application.TestUtils.Builders.DTOs;
using OrderFlow.Infrastructure.Alerts;
using OrderFlow.Infrastructure.TestUtils.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.Alerts;

[Collection("WireMock collection")]
public class AlertServiceTests(WireMockDriverFixture fixture) : IClassFixture<WireMockDriverFixture>, IAsyncLifetime
{
    private const string AlertServicePath = "/alerts";
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        { PropertyNameCaseInsensitive = true };

    public ValueTask InitializeAsync() => fixture.ResetLogEntries();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SendAsync_ShouldPostAlertRequest()
    {
        // Arrange
        const string type = "alert type";
        const string correlationId = "correlationId";
        const string messageId = "message id";
        const string error = "FakeError";
        
        var request = new AlertRequestBuilder()
            .WithType(type)
            .WithCorrelationId(correlationId)
            .WithMessageId(messageId)
            .WithError(error)
            .Build();
        
        fixture.Driver.ConfigureSuccessfulResponse(AlertServicePath);
        
        var sut = new AlertService(fixture.Driver.CreateClient());

        // Act
        await sut.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        fixture.LogEntries.Should().ContainSingle();
        var messageBody = fixture.LogEntries.Single().RequestMessage?.Body;

        messageBody.Should().NotBeNullOrEmpty();
        var requestMessage = JsonSerializer.Deserialize<AlertRequest>(messageBody, _jsonSerializerOptions);

        requestMessage.Should().NotBeNull();
        requestMessage.Type.Should().Be(type);
        requestMessage.CorrelationId.Should().Be(correlationId);
        requestMessage.Error.Should().Be(error);
        requestMessage.MessageId.Should().Be(messageId);
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenApiReturnsError()
    {
        // Arrange
        var request = new AlertRequestBuilder().Build();
        fixture.Driver.ConfigureInternalServerError(AlertServicePath);
        var sut = new AlertService(fixture.Driver.CreateClient());
        
        // Act
        var act = () => sut.SendAsync(request, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should()
            .ThrowAsync<HttpRequestException>();
    }
}