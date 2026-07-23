using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using Shouldly;

namespace Mediaspot.UnitTests
{
    public class VideoAssetTests
    {
        [Fact]
        public void VideoAsset_Should_Be_Created_With_Valid_Metadata()
        {
            var externalId = "Video001";
            var metadata = new Metadata("Test Asset", "desc", "en");
            var duration = new Duration(TimeSpan.FromSeconds(240));
            var resolution = "1080p";
            var framerate = 24;
            var codec = "H264";


            var asset =  new VideoAsset(externalId, metadata,duration, resolution, framerate, "H264");

            asset.ShouldNotBeNull();
            asset.ExternalId.ShouldBe(externalId);
            asset.Metadata.ShouldBe(metadata);
            asset.Duration.ShouldBe(duration);
            asset.Resolution.ShouldBe("1080p");
            asset.FrameRate.ShouldBe(framerate);
            asset.Codec.ShouldBe(codec);
        }

        [Fact]
        public void VideoAsset_Should_Throw_When_Duration_Is_Not_Positive()
        {
            var externalId = "Video001";
            var metadata = new Metadata("Test Asset", "desc", "en");
            var duration = new Duration(TimeSpan.FromSeconds(-5));
            var resolution = "1080p";
            var framerate = 24;
            var codec = "H264";

            Action action = () => new VideoAsset(
                externalId,
                metadata,
                duration,
                resolution,
                framerate,
                codec);

            Should.Throw<ArgumentException>(action);
        }

        [Fact]
        public void VideoAsset_Should_Throw_When_Codec_Is_Not_Right()
        {
            var externalId = "Video001";
            var metadata = new Metadata("Test Asset", "desc", "en");
            var duration = new Duration(TimeSpan.FromSeconds(240));
            var resolution = "1080p";
            var framerate = 24;
            var codec = "H642";

            Action action = () => new VideoAsset(
                externalId,
                metadata,
                duration,
                resolution,
                framerate,
                codec);

            var exception = Should.Throw<ArgumentException>(action);

            exception.ParamName.ShouldBe("codec");
        }
    }
}
