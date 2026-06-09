using System.Text.Json;
using FluentAssertions;
using OrderFlow.Application.Alerts;
using OrderFlow.Infrastructure.Alerts;
using OrderFlow.Infrastructure.IntegrationTests.Fixtures;
using OrderFlow.Testing.Builders.DTOs;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace OrderFlow.Infrastructure.IntegrationTests.Alerts;

[Collection("WireMock collection")]
public class AlertServiceTests(WireMockFixture fixture)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        { PropertyNameCaseInsensitive = true };

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
        
        ConfigureSuccessfulResponse();
        
        var sut = new AlertService(fixture.CreateClient());

        // Act
        await sut.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
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
        ConfigureInternalServerError();
        var sut = new AlertService(fixture.CreateClient());
        
        // Act
        var act = () => sut.SendAsync(request, TestContext.Current.CancellationToken);
        
        // Assert
        await act.Should()
            .ThrowAsync<HttpRequestException>();
    }
    
    private void ConfigureSuccessfulResponse()
    {
        fixture.Server
            .Given(
                Request.Create()
                    .WithPath("/alerts")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200));
    }
    
    private void ConfigureInternalServerError()
    {
        fixture.Server
            .Given(
                Request.Create()
                    .WithPath("/alerts")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(500));
    }
}