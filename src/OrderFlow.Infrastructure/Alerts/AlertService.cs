using System.Net.Http.Json;
using OrderFlow.Application.Alerts;

namespace OrderFlow.Infrastructure.Alerts;

public class AlertService(HttpClient httpClient) : IAlertService
{
    public async Task SendAsync(AlertRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/alerts", request);
        response.EnsureSuccessStatusCode();
    }
}