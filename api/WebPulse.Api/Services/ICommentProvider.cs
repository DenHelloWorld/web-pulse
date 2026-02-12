using System.Threading.Channels;
using WebPulse.Api.Models;

namespace WebPulse.Api.Services;

public interface ICommentProvider
{
    string ProviderName { get; }
    Task GetCommentsAsync(ChannelWriter<RawComment> writer, CancellationToken cancellationToken = default);
}
