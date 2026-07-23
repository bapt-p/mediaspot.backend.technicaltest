using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using Mediaspot.Domain.Assets.Events;
using Shouldly;

namespace Mediaspot.UnitTests;

public class AssetTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_And_Raise_AssetCreated()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";


        var metadata = new Metadata("title", "desc", "en");
        var asset = new VideoAsset("ext-1", metadata, duration, resolution, framerate, codec);

        asset.ExternalId.ShouldBe("ext-1");
        asset.Metadata.ShouldBe(metadata);
        asset.DomainEvents.OfType<AssetCreated>().Any(ac => ac.AssetId == asset.Id).ShouldBeTrue();
    }

    [Fact]
    public void RegisterMediaFile_Should_Add_File_And_Raise_Event()
    {
        var duration = Duration.FromSeconds(10);
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";


        var asset = new VideoAsset("ext-2", new Metadata("t", null, null), duration, resolution, framerate, codec);
        var path = new FilePath("/file.mp4");
        
        var mf = asset.RegisterMediaFile(path, duration);

        asset.MediaFiles.ShouldContain(mf);
        asset.DomainEvents.OfType<MediaFileRegistered>().Any(reg => reg.AssetId == asset.Id && reg.MediaFileId == mf.Id.Value).ShouldBeTrue();
    }

    [Fact]
    public void UpdateMetadata_Should_Set_Metadata_And_Raise_Event()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var asset = new VideoAsset("ext-3", new Metadata("t", null, null), duration, resolution, framerate, codec);
        var newMeta = new Metadata("new", "d", "fr");

        asset.UpdateMetadata(newMeta);

        asset.Metadata.ShouldBe(newMeta);
        asset.DomainEvents.OfType<MetadataUpdated>().Any(mu => mu.AssetId == asset.Id).ShouldBeTrue();
    }

    [Fact]
    public void UpdateMetadata_Should_Throw_If_Title_Empty()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var asset = new VideoAsset("ext-4", new Metadata("t", null, null), duration, resolution, framerate, codec);
        var invalid = new Metadata("", null, null);

        Should.Throw<ArgumentException>(() => asset.UpdateMetadata(invalid));
    }

    [Fact]
    public void Archive_Should_Set_Archived_And_Raise_Event()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var asset = new VideoAsset("ext-5", new Metadata("t", null, null), duration, resolution, framerate, codec);
        asset.Archive(_ => false);

        asset.Archived.ShouldBeTrue();
        asset.DomainEvents.OfType<AssetArchived>().Any(aa => aa.AssetId == asset.Id).ShouldBeTrue();
    }

    [Fact]
    public void Archive_Should_Throw_If_ActiveJobs()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264";

        var asset = new VideoAsset("ext-6", new Metadata("t", null, null), duration, resolution, framerate,codec);
        Should.Throw<InvalidOperationException>(() => asset.Archive(_ => true));
    }

    [Fact]
    public void Archive_Should_Be_Idempotent()
    {
        var duration = new Duration(TimeSpan.FromSeconds(240));
        var resolution = "1080p";
        var framerate = 24;
        var codec = "H264"; 

        var asset = new VideoAsset("ext-7", new Metadata("t", null, null), duration, resolution, framerate, codec);
        asset.Archive(_ => false);
        asset.Archive(_ => false);
        asset.Archived.ShouldBeTrue();
        asset.DomainEvents.OfType<AssetArchived>().Count().ShouldBe(1);
    }
}
