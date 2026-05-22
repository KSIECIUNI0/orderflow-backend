namespace OrderFlow.Worker.Shared.AlertService;

public interface IAlertService
{
    Task SendAsync(AlertRequest request);
}