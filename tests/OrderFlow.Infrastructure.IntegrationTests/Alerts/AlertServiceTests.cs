using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Alerts;
using OrderFlow.Infrastructure.Alerts;
using OrderFlow.Testing.Builders.DTOs;
using OrderFlow.Testing.Drivers;
using OrderFlow.Testing.Fixtures;

namespace OrderFlow.Infrastructure.IntegrationTests.Alerts;

[Collection("WireMock collection")]
public class AlertServiceTests(WireMockFixture fixture) : IAsyncLifetime
{
    private const string AlertServicePath = "/alerts";
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        { PropertyNameCaseInsensitive = true };

    private readonly WireMockDriver _wireMockDriver = new(fixture.Server);

    public ValueTask InitializeAsync() => _wireMockDriver.ResetLogEntries();

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
        
        _wireMockDriver.ConfigureSuccessfulResponse(AlertServicePath);
        
        var sut = new AlertService(_wireMockDriver.CreateClient());

        // Act
        await sut.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        fixture.Server.LogEntries.Should().ContainSingle();
        var messageBody = fixture.Server.LogEntries.Single().RequestMessage?.Body;

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
        _wireMockDriver.ConfigureInternalServerError(AlertServicePath);
        var sut = new AlertService(_wireMockDriver.CreateClient());
        
        // Act
        var act = () => sut.SendAsync(request, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should()
            .ThrowAsync<HttpRequestException>();
    }
}