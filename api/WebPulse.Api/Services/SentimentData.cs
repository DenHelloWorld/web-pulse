namespace WebPulse.Api.Services;

public class SentimentData
{
    public string Text { get; set; } = string.Empty;
    public bool Label { get; set; } // true for positive, false for negative
}

public class SentimentPrediction
{
    public bool Prediction { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}
