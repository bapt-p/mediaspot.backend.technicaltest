using Mediaspot.Application.Common;
using Mediaspot.Domain.Transcoding;
using Microsoft.EntityFrameworkCore;

namespace Mediaspot.Infrastructure.Persistence;

public sealed class TranscodeJobRepository(MediaspotDbContext db) : ITranscodeJobRepository
{
    public async Task AddAsync(TranscodeJob job, CancellationToken ct) => await db.TranscodeJobs.AddAsync(job, ct);

    public Task<bool> HasActiveJobsAsync(Guid assetId, CancellationToken ct)
        => db.TranscodeJobs.AnyAsync(j => j.AssetId == assetId && (j.Status == TranscodeStatus.Pending || j.Status == TranscodeStatus.Running), ct);

    public Task<TranscodeJob?> GetByIdAsync(Guid jobId, CancellationToken ct) 
        => db.TranscodeJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
    public async Task<Guid?> GetNextPendingJobIdAsync(CancellationToken ct)
        => (await db.TranscodeJobs.FirstOrDefaultAsync(j => j.Status == TranscodeStatus.Pending, ct))?.Id;
}
