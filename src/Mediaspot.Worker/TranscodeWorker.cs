using Mediaspot.Application.Common;
using Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.FailTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.StartTranscodeJob;
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

                    try
                    {
                        // Simulate the transcode processing requested by the exercise.
                        await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        stoppingToken);
                        await sender.Send(new CompleteTranscodeJobCommand(jobId.Value), stoppingToken);

                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to mark job {JobId} as completed", jobId.Value);
                        await sender.Send(new FailTranscodeJobCommand(jobId.Value), stoppingToken);

                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("TranscodeWorker is shutting down");
            }
        }
    }
}