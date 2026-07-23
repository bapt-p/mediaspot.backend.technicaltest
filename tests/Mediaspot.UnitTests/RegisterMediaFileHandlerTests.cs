using Mediaspot.Application.Assets.Commands.RegisterMediaFile;
using Mediaspot.Application.Common;
using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using Moq;
using Shouldly;

namespace Mediaspot.UnitTests;

public class RegisterMediaFileHandlerTests
{
    [Fact]
    public async Task Handle_Should_Register_MediaFile_And_Save()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var asset = new VideoAsset("ext", new Metadata("t", null, null), duration, resolution, framerate, codec);
        var repo = new Mock<IAssetRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(asset);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = new RegisterMediaFileHandler(repo.Object, uow.Object);
        var cmd = new RegisterMediaFileCommand(asset.Id, "/file.mp4", 10);

        var mfId = await handler.Handle(cmd, CancellationToken.None);

        asset.MediaFiles.ShouldContain(mf => mf.Id.Value == mfId);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_If_Asset_Not_Found()
    {
        var repo = new Mock<IAssetRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Asset?)null);
        var handler = new RegisterMediaFileHandler(repo.Object, uow.Object);
        var cmd = new RegisterMediaFileCommand(Guid.NewGuid(), "/file.mp4", 10);

        await Should.ThrowAsync<KeyNotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
    }
}
