using Mediaspot.Domain.Common;
namespace Mediaspot.Domain.Transcoding.Events
{
    public sealed record TranscodeJobStarted(Guid JobId) : IDomainEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
