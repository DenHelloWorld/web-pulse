using Microsoft.AspNetCore.SignalR;

namespace WebPulse.Api.Hubs;

// Наш объект данных
public record Pulse(double Sentiment, string Message, string Color);

public class PulseHub : Hub
{
    // Сюда можно добавить логику авторизации или групп, если понадобится
}
