using Microsoft.AspNetCore.SignalR;

namespace WebPulse.Api.Hubs;

// Наш объект данных
public record Pulse(double Sentiment, string Message, string Color, string Source, DateTime Timestamp);

public class PulseHub : Hub
{
    // Группа для всех подключенных клиентов
    public const string PulseGroupName = "AllPulses";
    
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, PulseGroupName);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, PulseGroupName);
        await base.OnDisconnectedAsync(exception);
    }
    
    // Метод для отправки pulse всем клиентам
    public async Task SendPulseToAll(Pulse pulse)
    {
        await Clients.Group(PulseGroupName).SendAsync("ReceivePulse", pulse);
    }
}
