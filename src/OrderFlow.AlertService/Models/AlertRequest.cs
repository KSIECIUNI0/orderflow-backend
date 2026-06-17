namespace OrderFlow.AlertService.Models;

public record AlertRequest(
    string Type,
    string MessageId,
    string Error);