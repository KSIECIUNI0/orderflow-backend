using OrderFlow.Application.Alerts.DTOs;

namespace OrderFlow.Application.Alerts;

public interface IAlertService
{
    Task SendAsync(AlertRequest request, CancellationToken cancellationToken = default);
}