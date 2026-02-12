using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using WebPulse.Api.Hubs;
using WebPulse.Api.Constants;

namespace WebPulse.Api.Services;

public class PulseGenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PulseGenerationService> _logger;
    private readonly Channel<PulseData> _pulseChannel;
    private readonly List<ICommentProvider> _providers;

    public PulseGenerationService(IServiceProvider serviceProvider, ILogger<PulseGenerationService> logger, IEnumerable<ICommentProvider> providers)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pulseChannel = Channel.CreateUnbounded<PulseData>();
        _providers = providers.ToList();
        
        _logger.LogInformation("PulseGenerationService initialized with {ProviderCount} providers", _providers.Count);
    }

    public void RegisterProvider(ICommentProvider provider)
    {
        _providers.Add(provider);
        _logger.LogInformation("Registered provider: {ProviderName}", provider.ProviderName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pulse Generation Service started");
        
        // Запускаем обработчик канала
        var processorTask = ProcessPulsesAsync(stoppingToken);
        
        // Периодически опрашиваем провайдеров
        var providerTask = PollProvidersAsync(stoppingToken);
        
        await Task.WhenAll(processorTask, providerTask);
    }

    private async Task ProcessPulsesAsync(CancellationToken stoppingToken)
    {
        await foreach (var pulseData in _pulseChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PulseHub>>();
                var sentimentService = scope.ServiceProvider.GetRequiredService<ISentimentAnalysisService>();
                
                var sentiment = await sentimentService.AnalyzeSentimentAsync(pulseData.Text);
                var color = GetColorFromSentiment(sentiment);
                
                var pulse = new Pulse(
                    Sentiment: sentiment,
                    Message: pulseData.Text.Length > 100 ? pulseData.Text.Substring(0, 100) + "..." : pulseData.Text,
                    FullText: pulseData.Text,
                    Color: color,
                    Source: pulseData.Source,
                    Author: pulseData.Author,
                    Url: pulseData.Url,
                    Timestamp: pulseData.Timestamp
                );
                
                _logger.LogDebug("Created pulse: {Text} (Sentiment: {Sentiment})", 
                    pulse.Message, pulse.Sentiment);
                
                await hubContext.Clients.Group(PulseHub.PulseGroupName).SendAsync(ProviderConstants.SignalR.ReceiveMethod, pulse, stoppingToken);
                
                _logger.LogDebug("Sent pulse: {Text} -> {Sentiment:F2}", pulseData.Text.Substring(0, Math.Min(30, pulseData.Text.Length)), sentiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pulse: {Text}", pulseData.Text);
            }
        }
    }

    private async Task PollProvidersAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tasks = _providers.Select(async provider =>
                {
                    try
                    {
                        var comments = await provider.GetCommentsAsync(stoppingToken);
                        foreach (var comment in comments)
                        {
                            await _pulseChannel.Writer.WriteAsync(new PulseData(
                                Text: comment.Text,
                                Source: comment.Source,
                                Author: comment.Author,
                                Url: comment.Url,
                                Timestamp: comment.Timestamp
                            ), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error polling provider {ProviderName}", provider.ProviderName);
                    }
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in provider polling loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(ProviderConstants.Reddit.PollingIntervalSeconds), stoppingToken);
        }
    }

    private static string GetColorFromSentiment(float sentiment)
    {
        return sentiment switch
        {
            > ProviderConstants.UI.PositiveThreshold => ProviderConstants.UI.PositiveColor,
            < ProviderConstants.UI.NegativeThreshold => ProviderConstants.UI.NegativeColor,
            _ => ProviderConstants.UI.NeutralColor
        };
    }

    public override void Dispose()
    {
        _pulseChannel.Writer.Complete();
        base.Dispose();
    }
}

public record PulseData(
    string Text, 
    string Source, 
    string Author,
    string Url,
    DateTime Timestamp
);
