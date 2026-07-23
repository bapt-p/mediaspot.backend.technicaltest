using Mediaspot.Domain.Transcoding;

namespace Mediaspot.Application.Common;

public interface ITranscodeJobRepository
{
    Task AddAsync(TranscodeJob job, CancellationToken ct);
    Task<bool> HasActiveJobsAsync(Guid assetId, CancellationToken ct);
    Task<TranscodeJob?> GetByIdAsync(Guid jobId, CancellationToken ct); 
    
    Task<Guid?> GetNextPendingJobIdAsync(CancellationToken ct);
}
