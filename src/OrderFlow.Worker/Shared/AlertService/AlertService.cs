using System.Net.Http.Json;

namespace OrderFlow.Worker.Shared.AlertService;

public class AlertService : IAlertService
{
    private readonly HttpClient _httpClient;

    public AlertService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendAsync(AlertRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/alerts", request);
        response.EnsureSuccessStatusCode();
    }
}