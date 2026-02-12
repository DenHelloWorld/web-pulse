using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using WebPulse.Api.Constants;
using WebPulse.Api.Models;

namespace WebPulse.Api.Services;

public class RedditProvider : ICommentProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedditProvider> _logger;
    
    public string ProviderName => "Reddit";

    public RedditProvider(HttpClient httpClient, ILogger<RedditProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Reddit API headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", ProviderConstants.Reddit.UserAgent);
    }

    public async Task GetCommentsAsync(ChannelWriter<RawComment> writer, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching posts from Reddit...");
            
            // Use a more reliable endpoint with explicit JSON extension
            var endpoint = $"{ProviderConstants.Reddit.BaseUrl}/r/{ProviderConstants.Reddit.Subreddit}/new.json?limit=10&raw_json=1";
            _logger.LogDebug("Requesting URL: {Url}", endpoint);
            
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Reddit API returned status: {StatusCode}. Response: {Response}", 
                    response.StatusCode, errorContent);
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Received Reddit response. Length: {Length} chars", content.Length);
            
            // Log first 200 chars for debugging
            _logger.LogTrace("Response start: {ResponseStart}", 
                content.Length > 200 ? content[..200] + "..." : content);
            
            RedditResponse? redditData = null;
            try 
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };
                
                redditData = JsonSerializer.Deserialize<RedditResponse>(content, options);
                _logger.LogInformation("Successfully deserialized Reddit response. Found {Count} posts", 
                    redditData?.Data?.Children?.Count ?? 0);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize Reddit response. First 500 chars: {ResponseStart}", 
                    content.Length > 500 ? content[..500] : content);
                return;
            }

            if (redditData?.Data?.Children == null)
            {
                _logger.LogWarning("Invalid Reddit response format");
                return;
            }

            foreach (var child in redditData.Data.Children.Where(c => c?.Kind == "t3"))
            {
                var post = child!.Data;
                if (post != null && !string.IsNullOrWhiteSpace(post.Title))
                {
                    var postText = !string.IsNullOrEmpty(post.Selftext) ? post.Selftext : post.Title;
                    
                    var rawComment = new RawComment
                    {
                        Text = post.Title,
                        Source = $"Reddit/r/{post.Subreddit}",
                        Author = post.Author ?? "[deleted]",
                        Url = string.IsNullOrEmpty(post.Url) ? $"https://reddit.com{post.Permalink}" : post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)post.CreatedUtc).DateTime
                    };
                    
                    await writer.WriteAsync(rawComment, cancellationToken);
                }
            }

            _logger.LogDebug("Retrieved and sent {Count} posts to channel", redditData.Data.Children.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from Reddit");
        }
    }
}

// Reddit API response models
public class RedditResponse
{
    [JsonPropertyName("data")]
    public RedditData? Data { get; set; }
}

public class RedditData
{
    [JsonPropertyName("children")]
    public List<RedditChild> Children { get; set; } = new();
    
    [JsonPropertyName("after")]
    public string? After { get; set; }
    
    [JsonPropertyName("before")]
    public string? Before { get; set; }
}

public class RedditChild
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public RedditPost? Data { get; set; }
}

public class RedditPost
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("permalink")]
    public string Permalink { get; set; } = string.Empty;
    
    [JsonPropertyName("created_utc")]
    public double CreatedUtc { get; set; }
    
    [JsonPropertyName("selftext")]
    public string Selftext { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("num_comments")]
    public int NumComments { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("is_self")]
    public bool IsSelf { get; set; }
}
