namespace WebPulse.Api.Models;

public class SentimentData
{
    public string Text { get; set; } = string.Empty;
}

public class SentimentPrediction
{
    public float Score { get; set; }
    public float Probability { get; set; }
}

public class RawComment
{
    public string Text { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
