using Microsoft.AspNetCore.SignalR;
using WebPulse.Api.Hubs;

namespace WebPulse.Api.Services;

public class DataHarvester : BackgroundService
{
    private readonly IHubContext<PulseHub> _hubContext;
    private readonly Random _random = new();

    public DataHarvester(IHubContext<PulseHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Генерируем случайный sentiment от -1.0 до 1.0
            double sentiment = (_random.NextDouble() * 2) - 1;
            string message = sentiment > 0 ? "Positive vibe!" : "System alert: Negative!";
            string color = sentiment > 0 ? "#0000FF" : "#FF0000";

            await _hubContext.Clients.All.SendAsync("ReceivePulse",
                new Pulse(sentiment, message, color, "DataHarvester", DateTime.UtcNow),
                stoppingToken);

            await Task.Delay(2000, stoppingToken); // Пауза 2 секунды
        }
    }
}
