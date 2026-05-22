using OrderFlow.AlertService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/alerts", (
    AlertRequest request,
    ILogger<Program> logger) =>
{
    logger.LogError(
        """
        ALERT RECEIVED
        TYPE: {Type}
        MessageId: {MessageId}
        Error: {Error}
        """,
        request.Type,
        request.MessageId,
        request.Error);

    return Results.Ok();
});

await app.RunAsync();