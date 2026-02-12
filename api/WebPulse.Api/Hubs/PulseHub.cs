using Microsoft.AspNetCore.SignalR;
using WebPulse.Api.Constants;

namespace WebPulse.Api.Hubs;

// Наш объект данных
public record Pulse(
    double Sentiment, 
    string Message, 
    string FullText,
    string Color, 
    string Source, 
    string Author,
    string Url,
    DateTime Timestamp
);

public class PulseHub : Hub
{
    // Группа для всех подключенных клиентов
    public const string PulseGroupName = ProviderConstants.SignalR.GroupName;
    
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
        await Clients.Group(PulseGroupName).SendAsync(ProviderConstants.SignalR.ReceiveMethod, pulse);
    }
}
