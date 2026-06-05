namespace OrderFlow.Application.Alerts;

public interface IAlertService
{
    Task SendAsync(AlertRequest request);
}