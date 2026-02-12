using Microsoft.ML;
using Microsoft.Extensions.ObjectPool;
using System.IO;
using System.Text.RegularExpressions;
using WebPulse.Api.Models;

namespace WebPulse.Api.Services;

public interface ISentimentAnalysisService
{
    Task<float> AnalyzeSentimentAsync(string text);
}

public class PredictionEngineWrapper
{
    public PredictionEngine<SentimentData, SentimentPrediction> Engine { get; set; } = null!;
}

public class PredictionEnginePolicy : IPooledObjectPolicy<PredictionEngineWrapper>
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;

    public PredictionEnginePolicy(MLContext mlContext, ITransformer model)
    {
        _mlContext = mlContext;
        _model = model;
    }

    public PredictionEngineWrapper Create()
    {
        return new PredictionEngineWrapper
        {
            Engine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model)
        };
    }

    public bool Return(PredictionEngineWrapper obj)
    {
        return true;
    }
}

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly ObjectPool<PredictionEngineWrapper> _enginePool;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
    {
        _logger = logger;
        _enginePool = InitializeEnginePool();
    }

    private ObjectPool<PredictionEngineWrapper> InitializeEnginePool()
    {
        var mlContext = new MLContext();
        var modelPath = Path.Combine(AppContext.BaseDirectory, "Models", "sentiment.zip");

        if (!File.Exists(modelPath))
        {
            _logger.LogWarning("🚨 Файл модели не найден по пути: {Path}. Будет использован Dummy-анализ.", modelPath);
        }

        ITransformer model = mlContext.Model.Load(modelPath, out var modelInputSchema);

        return ObjectPool.Create(new PredictionEnginePolicy(mlContext, model));
    }

    public async Task<float> AnalyzeSentimentAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        string cleanText = Regex.Replace(text.Replace("\n", " ").Replace("\r", " ").Trim(), @"\s+", " ");

        var wrapper = _enginePool.Get();
        try
        {
            var prediction = wrapper.Engine.Predict(new SentimentData { Text = cleanText });

            float score = (prediction.Probability - 0.5f) * 2;

            _logger.LogDebug("ML.NET Analysis: '{Text}' -> Score: {Score:F2}",
                cleanText.Length > 30 ? cleanText[..30] : cleanText, score);

            return score;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML.NET Prediction failed. Falling back to dummy.");
            return await AnalyzeSentimentDummyAsync(text);
        }
        finally
        {
            _enginePool.Return(wrapper);
        }
    }

    private async Task<float> AnalyzeSentimentDummyAsync(string text)
    {
        await Task.Delay(1);

        // Улучшенная dummy логика как fallback
        var sentimentWords = new Dictionary<string, float>
        {
            {"amazing", 0.8f}, {"awesome", 0.9f}, {"great", 0.7f}, {"love", 0.9f},
            {"excellent", 0.8f}, {"fantastic", 0.9f}, {"wonderful", 0.8f}, {"good", 0.6f},
            {"bad", -0.6f}, {"terrible", -0.8f}, {"awful", -0.9f}, {"hate", -0.9f},
            {"horrible", -0.8f}, {"disgusting", -0.9f}, {"worst", -0.8f}, {"evil", -0.7f},
            {"recommend", 0.7f}, {"disappoint", -0.7f}, {"outstanding", 0.9f}, {"waste", -0.8f}
        };

        var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sentimentScore = 0f;
        var wordCount = 0;

        foreach (var word in words)
        {
            if (sentimentWords.TryGetValue(word, out var score))
            {
                sentimentScore += score;
                wordCount++;
            }
        }

        if (wordCount == 0)
        {
            sentimentScore = Random.Shared.NextSingle() * 2f - 1f;
        }
        else
        {
            sentimentScore /= wordCount;
        }

        return Math.Max(-1f, Math.Min(1f, sentimentScore));
    }
}
