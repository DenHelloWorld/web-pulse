namespace WebPulse.Api.Services;

public interface ICommentProvider
{
    string ProviderName { get; }
    Task<IEnumerable<CommentData>> GetCommentsAsync(CancellationToken cancellationToken = default);
}

public record CommentData(
    string Text,
    string FullText,
    string Source,
    DateTime Timestamp,
    string Author,
    string Url
);
