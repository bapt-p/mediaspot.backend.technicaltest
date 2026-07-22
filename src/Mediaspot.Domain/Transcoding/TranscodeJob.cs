using Mediaspot.Domain.Common;
using Mediaspot.Domain.Transcoding.Events;

namespace Mediaspot.Domain.Transcoding;

public enum TranscodeStatus { Pending, Running, Succeeded, Failed }

public sealed class TranscodeJob : AggregateRoot
{
    public Guid AssetId { get; private set; }
    public Guid MediaFileId { get; private set; }
    public string Preset { get; private set; }
    public TranscodeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TranscodeJob() { AssetId = Guid.Empty; MediaFileId = Guid.Empty; Preset = string.Empty; }

    public TranscodeJob(Guid assetId, Guid mediaFileId, string preset)
    {
        var now = DateTime.UtcNow;
        AssetId = assetId; MediaFileId = mediaFileId; Preset = preset; Status = TranscodeStatus.Pending; CreatedAt = now; UpdatedAt = now;
    }

    public void MarkRunning()
    {
        if (Status != TranscodeStatus.Pending)
        {
            throw new InvalidOperationException("A transcodeJob can only be started when it is pending");
        }

        Status = TranscodeStatus.Running;
        UpdatedAt = DateTime.UtcNow;

        Raise(new TranscodeJobStarted(Id));
    }
    public void MarkSucceeded()
    {
        if (Status != TranscodeStatus.Running)
        {
            throw new InvalidOperationException("A transcodeJob can only be completed when it is running");
        }

        Status = TranscodeStatus.Succeeded;
        UpdatedAt = DateTime.UtcNow;

        Raise(new TranscodeJobCompleted(Id));
    }
    public void MarkFailed()
    {
        if (Status != TranscodeStatus.Running)
        {
            throw new InvalidOperationException("A transcodeJob can only be failed when it is running");
        }

        Status = TranscodeStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        Raise(new TranscodeJobFailed(Id));
    }
}