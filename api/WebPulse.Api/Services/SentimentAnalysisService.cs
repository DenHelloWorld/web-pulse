using Microsoft.ML;
using Microsoft.ML.Data;

namespace WebPulse.Api.Services;

public interface ISentimentAnalysisService
{
    Task<float> AnalyzeSentimentAsync(string text);
}

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly ILogger<SentimentAnalysisService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;

    public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        
        // Инициализируем модель с предобученными данными
        InitializeModel();
    }

    private void InitializeModel()
    {
        try
        {
            // Создаем обучающие данные (в реальном проекте это будет загружаться из файла)
            var trainingData = new List<SentimentData>
            {
                new() { Text = "This is amazing! I love it", Label = true },
                new() { Text = "Great work everyone", Label = true },
                new() { Text = "Excellent performance", Label = true },
                new() { Text = "Love the new features", Label = true },
                new() { Text = "Fantastic job", Label = true },
                new() { Text = "I hate this so much", Label = false },
                new() { Text = "Terrible decision", Label = false },
                new() { Text = "Worst experience ever", Label = false },
                new() { Text = "Awful user interface", Label = false },
                new() { Text = "Horrible product", Label = false },
                new() { Text = "Not bad", Label = true },
                new() { Text = "Could be better", Label = false },
                new() { Text = "Pretty good", Label = true },
                new() { Text = "Quite disappointing", Label = false },
                new() { Text = "Outstanding quality", Label = true },
                new() { Text = "Complete waste of time", Label = false },
                new() { Text = "Highly recommend", Label = true },
                new() { Text = "Never buying again", Label = false },
                new() { Text = "Above expectations", Label = true },
                new() { Text = "Below average", Label = false }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Создаем pipeline для обработки текста
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "Text")
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            // Обучаем модель
            _model = pipeline.Fit(dataView);
            
            // Создаем prediction engine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            
            _logger.LogInformation("ML.NET sentiment analysis model initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ML.NET model, falling back to dummy logic");
        }
    }

    public async Task<float> AnalyzeSentimentAsync(string text)
    {
        await Task.Delay(1); // Минимальная задержка для async
        
        try
        {
            if (_predictionEngine == null)
            {
                // Fallback к dummy логике если модель не загрузилась
                return await AnalyzeSentimentDummyAsync(text);
            }

            var inputData = new SentimentData { Text = text };
            var prediction = _predictionEngine.Predict(inputData);
            
            // Конвертируем binary prediction в sentiment score (-1.0 to 1.0)
            var sentimentScore = prediction.Prediction ? 
                Math.Max(0.1f, prediction.Probability) : // Positive: 0.1 to 1.0
                Math.Min(-0.1f, -prediction.Probability); // Negative: -1.0 to -0.1
            
            _logger.LogDebug("ML.NET Analysis: '{Text}' -> {Score:F2} (Confidence: {Prob:F2})", 
                text.Substring(0, Math.Min(30, text.Length)), sentimentScore, prediction.Probability);
            
            return sentimentScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ML.NET sentiment analysis, falling back to dummy logic");
            return await AnalyzeSentimentDummyAsync(text);
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
