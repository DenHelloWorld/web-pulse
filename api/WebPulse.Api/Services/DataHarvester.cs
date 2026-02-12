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
        var fakeAuthors = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry" };

        while (!stoppingToken.IsCancellationRequested)
        {
            // Генерируем случайный sentiment от -1.0 до 1.0
            double sentiment = (_random.NextDouble() * 2) - 1;
            string message = sentiment > 0 ? "Positive vibe!" : "System alert: Negative!";
            string color = sentiment > 0 ? "#0000FF" : "#FF0000";
            string author = fakeAuthors[_random.Next(fakeAuthors.Length)];

            await _hubContext.Clients.All.SendAsync("ReceivePulse",
                new Pulse(
                    Sentiment: sentiment,
                    Message: message,
                    FullText: message,
                    Color: color,
                    Source: "DataHarvester",
                    Author: author,
                    Url: "",
                    Timestamp: DateTime.UtcNow
                ),
                stoppingToken);

            await Task.Delay(2000, stoppingToken); // Пауза 2 секунды
        }
    }
}
