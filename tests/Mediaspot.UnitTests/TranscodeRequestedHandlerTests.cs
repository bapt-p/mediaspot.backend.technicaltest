using Mediaspot.Application.Common;
using Mediaspot.Application.Events;
using Mediaspot.Application.Transcoding.Commands.StartTranscodeJob;
using Mediaspot.Domain.Assets.Events;
using Mediaspot.Domain.Transcoding;
using Moq;
using Shouldly;

namespace Mediaspot.UnitTests;

public class TranscodeRequestedHandlerTests
{
    [Fact]
    public async Task Handle_Should_Add_TranscodeJob_And_Save()
    {
        var repo = new Mock<ITranscodeJobRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.AddAsync(It.IsAny<TranscodeJob>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new TranscodeRequestedHandler(repo.Object, uow.Object);
        var evt = new TranscodeRequested(Guid.NewGuid(), Guid.NewGuid(), "preset");

        await handler.Handle(evt, CancellationToken.None);

        repo.Verify(r => r.AddAsync(It.Is<TranscodeJob>(j => j.AssetId == evt.AssetId && j.MediaFileId == evt.MediaFileId && j.Preset == evt.TargetPreset), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Mark_Job_As_Running_And_Save_Changes()
    {
        var repo = new Mock<ITranscodeJobRepository>();
        var uow = new Mock<IUnitOfWork>();

        var transcodeJob = new TranscodeJob(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "720p");

        repo
            .Setup(r => r.GetByIdAsync(
                transcodeJob.Id,It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcodeJob);

        uow
            .Setup(u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new StartTranscodeJobCommandHandler(
            repo.Object,
            uow.Object);

        var command = new StartTranscodeJobCommand(
            transcodeJob.Id);

        await handler.Handle(
            command,
            CancellationToken.None);

        transcodeJob.Status.ShouldBe(TranscodeStatus.Running);

        repo.Verify(
            r => r.GetByIdAsync(
                transcodeJob.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
