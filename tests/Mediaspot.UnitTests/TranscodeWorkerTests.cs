using MediatR;
using Mediaspot.Application.Common;
using Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.FailTranscodeJob;
using Mediaspot.Application.Transcoding.Commands.StartTranscodeJob;
using Mediaspot.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public sealed class TranscodeWorkerTests
{
    [Fact]
    public async Task Worker_Should_Start_And_Complete_Pending_Job()
    {
        var jobId = Guid.NewGuid();

        var jobs = new Mock<ITranscodeJobRepository>();
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
}