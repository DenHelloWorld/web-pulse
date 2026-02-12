namespace WebPulse.Api.Services;

public interface ICommentProvider
{
    string ProviderName { get; }
    Task<IEnumerable<CommentData>> GetCommentsAsync(CancellationToken cancellationToken = default);
}

public record CommentData(
    string Text,
    string Source,
    DateTime Timestamp,
    string? Author = null,
    string? Url = null
);
