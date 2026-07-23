using Mediaspot.Application.Common;
using Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.FailTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.StartTranscodeJob;
using Mediaspot.Domain.Assets;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mediaspot.Worker
{
    public sealed class TranscodeWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TranscodeWorker> logger)
    : BackgroundService
    {
        protected override async Task ExecuteAsync(
         CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // A hosted service is long-lived, while the repository,
                    // Unit of Work and DbContext are scoped dependencies.
                    // A new scope is therefore created for each processing iteration.
                    await using var scope =
                        scopeFactory.CreateAsyncScope();

                    // The worker delegates application use cases through MediatR
                    // instead of modifying the domain entity directly.
                    var sender = scope.ServiceProvider
                        .GetRequiredService<ISender>();

                    // The repository is resolved from the same scope so that it
                    // shares the expected scoped lifetime with the DbContext.
                    var jobs = scope.ServiceProvider
                        .GetRequiredService<ITranscodeJobRepository>();

                    // Poll the persistence layer for the next job waiting to be processed.
                    var jobId = await jobs.GetNextPendingJobIdAsync(stoppingToken);

                    if (jobId is null)
                    {
                        // No work is currently available. Wait before polling again
                        // to avoid continuously querying the database.
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    // The worker orchestrates the use case, while the command handler
                    // loads the aggregate, calls MarkRunning and persists the change.
                    await sender.Send(new StartTranscodeJobCommand(jobId.Value), stoppingToken);

                    var jobStarted = true;

                    try
                    {
                        // Retrieve the complete job and asset to handle type-specific processing.
                        var job = await jobs.GetByIdAsync(jobId.Value, stoppingToken);

                        if (job is null)
                        {
                            throw new KeyNotFoundException(
                                $"Transcode job '{jobId}' was not found.");
                        }

                        var assets = scope.ServiceProvider
                            .GetRequiredService<IAssetRepository>();

                        var asset = await assets.GetAsync(job.AssetId,stoppingToken);

                        if (asset is null)
                        {
                            throw new KeyNotFoundException(
                                $"Asset '{job.AssetId}' was not found.");
                        }

                        // Execute type-specific processing logic based on asset type.
                        switch (asset)
                        {
                            case VideoAsset videoAsset:
                                await ProcessVideoAsync(videoAsset, stoppingToken);
                                break;

                            case AudioAsset audioAsset:
                                await ProcessAudioAsync(audioAsset, stoppingToken);
                                break;

                            default:
                                throw new NotSupportedException(
                                    $"Asset type '{asset.GetType().Name}' is not supported.");
                        }

                        await sender.Send(new CompleteTranscodeJobCommand(jobId.Value), stoppingToken);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process transcode job {JobId}.", jobId.Value);
                        // TranscodeJob only permits the Running -> Failed transition,
                        // so Fail is sent only after Start succeeded.
                        if (jobStarted)
                        {
                            await sender.Send(new FailTranscodeJobCommand(jobId.Value), stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("TranscodeWorker is shutting down");
            }
        }
        private async Task ProcessVideoAsync(VideoAsset asset,CancellationToken cancellationToken)
        {
            // Dummy video-specific processing using video metadata.
            logger.LogInformation(
                "Processing video asset {AssetId}: {Resolution}, {FrameRate} FPS, {Codec}.",
                asset.Id,
                asset.Resolution,
                asset.FrameRate,
                asset.Codec);

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);
        }

        private async Task ProcessAudioAsync(
            AudioAsset asset,
            CancellationToken cancellationToken)
        {
            // Dummy audio-specific processing using audio metadata.
            logger.LogInformation(
                "Processing audio asset {AssetId}: {Bitrate} kbps, {SampleRate} Hz, {Channels} channels.",
                asset.Id,
                asset.Bitrate,
                asset.SampleRate,
                asset.Channels);

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);
        }
    }
}