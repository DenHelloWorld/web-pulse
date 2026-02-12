using System.Text.Json;
using System.Text.Json.Serialization;
using WebPulse.Api.Constants;

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

    public async Task<IEnumerable<CommentData>> GetCommentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Reddit's public JSON endpoint for /r/all/new
            var response = await _httpClient.GetAsync($"{ProviderConstants.Reddit.BaseUrl}{ProviderConstants.Reddit.ApiEndpoint}?limit={ProviderConstants.Reddit.DefaultLimit}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Reddit API returned status: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CommentData>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var redditData = JsonSerializer.Deserialize<RedditResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (redditData?.Data?.Children == null)
            {
                _logger.LogWarning("Invalid Reddit response format");
                return Enumerable.Empty<CommentData>();
            }

            var comments = new List<CommentData>();
            
            foreach (var child in redditData.Data.Children.Where(c => c?.Kind == "t3"))
            {
                var post = child!.Data;
                if (post != null && !string.IsNullOrWhiteSpace(post.Title))
                {
                    comments.Add(new CommentData(
                        Text: post.Title,
                        Source: $"{ProviderConstants.Reddit.SourcePrefix}{post.Subreddit}",
                        Timestamp: DateTimeOffset.FromUnixTimeSeconds(post.CreatedUtc).DateTime,
                        Author: post.Author,
                        Url: string.Format(ProviderConstants.Reddit.PostUrlFormat, post.Permalink)
                    ));
                }
            }

            _logger.LogDebug("Retrieved {Count} posts from Reddit", comments.Count);
            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from Reddit");
            return Enumerable.Empty<CommentData>();
        }
    }
}

// Reddit API response models
public class RedditResponse
{
    public RedditData? Data { get; set; }
}

public class RedditData
{
    public List<RedditChild>? Children { get; set; }
}

public class RedditChild
{
    public string Kind { get; set; } = string.Empty;
    public RedditPost? Data { get; set; }
}

public class RedditPost
{
    public string Title { get; set; } = string.Empty;
    public string Subreddit { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Permalink { get; set; } = string.Empty;
    public long CreatedUtc { get; set; }
    public string Selftext { get; set; } = string.Empty;
    public int Score { get; set; }
    public int NumComments { get; set; }
}
