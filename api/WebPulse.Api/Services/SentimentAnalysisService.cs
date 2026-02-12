namespace WebPulse.Api.Services;

public interface ISentimentAnalysisService
{
    Task<float> AnalyzeSentimentAsync(string text);
}

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly ILogger<SentimentAnalysisService> _logger;
    
    // Dummy слова для демонстрации
    private readonly Dictionary<string, float> _sentimentWords = new()
    {
        {"amazing", 0.8f}, {"awesome", 0.9f}, {"great", 0.7f}, {"love", 0.9f},
        {"excellent", 0.8f}, {"fantastic", 0.9f}, {"wonderful", 0.8f}, {"good", 0.6f},
        {"bad", -0.6f}, {"terrible", -0.8f}, {"awful", -0.9f}, {"hate", -0.9f},
        {"horrible", -0.8f}, {"disgusting", -0.9f}, {"worst", -0.8f}, {"evil", -0.7f}
    };

    public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<float> AnalyzeSentimentAsync(string text)
    {
        await Task.Delay(1); // Имитация работы ML модели
        
        var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sentimentScore = 0f;
        var wordCount = 0;

        foreach (var word in words)
        {
            if (_sentimentWords.TryGetValue(word, out var score))
            {
                sentimentScore += score;
                wordCount++;
            }
        }

        // Если не найдено сентиментных слов, генерируем случайный score
        if (wordCount == 0)
        {
            sentimentScore = Random.Shared.NextSingle() * 2f - 1f; // -1.0 to 1.0
        }
        else
        {
            sentimentScore /= wordCount;
        }

        // Ограничиваем диапазон
        sentimentScore = Math.Max(-1f, Math.Min(1f, sentimentScore));
        
        _logger.LogDebug("Text: {Text} -> Sentiment: {Score:F2}", text.Substring(0, Math.Min(50, text.Length)), sentimentScore);
        
        return sentimentScore;
    }
}
