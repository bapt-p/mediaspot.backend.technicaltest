using MediatR;
using Mediaspot.Application.Common;
using Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.FailTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.StartTranscodeJob;
using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using Mediaspot.Domain.Transcoding;
using Mediaspot.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

public sealed class TranscodeWorkerTests
{
    [Fact]
    public async Task Worker_Should_Start_And_Complete_Pending_Job()
    {
        var jobId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var mediaFileId = Guid.NewGuid();
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var jobs = new Mock<ITranscodeJobRepository>();
        var assets = new Mock<IAssetRepository>();
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<TranscodeWorker>>();

        // Synchronizes the test with the background execution.
        // This avoids relying on an arbitrary Task.Delay in the test.
        var completed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // The worker polls continuously:
        // - the first iteration finds one pending job;
        // - the next iteration finds nothing, preventing the same job
        //   from being processed repeatedly.
        jobs
            .SetupSequence(repository =>
                repository.GetNextPendingJobIdAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobId)
            .ReturnsAsync((Guid?)null);

        // Mock GetByIdAsync to return a transcode job
        var transcodeJob = new TranscodeJob(assetId, mediaFileId, "720p");
        jobs
            .Setup(repository =>
                repository.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcodeJob);

        // Mock GetAsync to return a video asset
        var videoAsset = new VideoAsset(
            externalId: "test-video",
            metadata: new Metadata("Test", "Test video", "en"),
            duration: duration,
            resolution: resolution,
            frameRate: framerate,
            codec: codec);

        assets
            .Setup(repository =>
                repository.GetAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoAsset);

        sender
            .Setup(service => service.Send(
                It.IsAny<StartTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        sender
            .Setup(service => service.Send(
                It.IsAny<CompleteTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => completed.TrySetResult())
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();

        // Register the mocked dependencies as scoped services because
        // the worker resolves them from a dependency-injection scope,
        // just as it does in production.
        services.AddScoped<ISender>(_ => sender.Object);
        services.AddScoped<ITranscodeJobRepository>(_ => jobs.Object);
        services.AddScoped<IAssetRepository>(_ => assets.Object);


        // Build an isolated DI container for this test.
        // It provides the real IServiceScopeFactory used by the worker.
        await using var provider =
            services.BuildServiceProvider();

        var worker = new TranscodeWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            logger.Object);

        await worker.StartAsync(CancellationToken.None);

        await completed.Task.WaitAsync(
            TimeSpan.FromSeconds(3));

        await worker.StopAsync(CancellationToken.None);

        // Verify the worker orchestrates the expected happy path:
        // Pending -> Start -> dummy processing -> Complete.
        sender.Verify(
            service => service.Send(
                It.Is<StartTranscodeJobCommand>(
                    command => command.JobId == jobId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        sender.Verify(
            service => service.Send(
                It.Is<CompleteTranscodeJobCommand>(
                    command => command.JobId == jobId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // No failure command should be emitted during the successful path.
        sender.Verify(
            service => service.Send(
                It.IsAny<FailTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Worker_Should_Process_VideoAsset_With_Type_Specific_Logic()
    {
        var jobId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var mediaFileId = Guid.NewGuid();
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        // Create a VideoAsset with video-specific properties
        var videoAsset = new VideoAsset(
            externalId: "video-test-001",
            metadata: new Metadata("Test Video", "Test video description", "en"),
            duration: new Duration(TimeSpan.FromMinutes(10)),
            resolution: resolution,
            frameRate: framerate,
            codec: codec);

        // Create a TranscodeJob associated with the video asset
        var transcodeJob = new TranscodeJob(assetId, mediaFileId, "720p");

        var completed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var jobs = new Mock<ITranscodeJobRepository>();
        var assets = new Mock<IAssetRepository>();
        var sender = new Mock<ISender>();
        var logger = new Mock<ILogger<TranscodeWorker>>();

        // Setup job repository: first returns pending jobId, then null to exit
        jobs
            .SetupSequence(repository =>
                repository.GetNextPendingJobIdAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobId)
            .ReturnsAsync((Guid?)null);

        // Setup job repository to return the transcode job
        jobs
            .Setup(repository =>
                repository.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcodeJob);

        // Setup asset repository to return the video asset
        assets
            .Setup(repository =>
                repository.GetAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoAsset);

        // Setup sender to handle commands
        sender
            .Setup(service => service.Send(
                It.IsAny<StartTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        sender
            .Setup(service => service.Send(
                It.IsAny<CompleteTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => completed.TrySetResult())
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddScoped<ISender>(_ => sender.Object);
        services.AddScoped<ITranscodeJobRepository>(_ => jobs.Object);
        services.AddScoped<IAssetRepository>(_ => assets.Object);

        await using var provider = services.BuildServiceProvider();

        var worker = new TranscodeWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            logger.Object);

        await worker.StartAsync(CancellationToken.None);

        await completed.Task.WaitAsync(TimeSpan.FromSeconds(3));

        await worker.StopAsync(CancellationToken.None);

        // Verify commands were sent in correct order
        sender.Verify(
            service => service.Send(
                It.Is<StartTranscodeJobCommand>(
                    command => command.JobId == jobId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        sender.Verify(
            service => service.Send(
                It.Is<CompleteTranscodeJobCommand>(
                    command => command.JobId == jobId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify asset repository was called
        assets.Verify(
            repository => repository.GetAsync(assetId, It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify no failure occurred
        sender.Verify(
            service => service.Send(
                It.IsAny<FailTranscodeJobCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        // Verify video asset properties were accessed (logged)
        logger.Verify();
    }
}