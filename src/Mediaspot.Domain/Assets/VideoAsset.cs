using Mediaspot.Domain.Assets.ValueObjects;

namespace Mediaspot.Domain.Assets
{
    public sealed class VideoAsset : Asset
    {
        private static readonly HashSet<string> SupportedCodecs =
         new(StringComparer.OrdinalIgnoreCase)
         {
            "H264",
            "H265",
            "VP9",
            "AV1"
         };

        public Duration Duration { get; private set; } = default!;
        public string Resolution { get; private set; } = string.Empty;
        public decimal FrameRate { get; private set; }
        public string Codec { get; private set; } = string.Empty;

        // Required by EF Core.
        private VideoAsset()
            : base()
        {
        }

        public VideoAsset(
            string externalId,
            Metadata metadata,
            Duration duration,
            string resolution,
            decimal frameRate,
            string codec)
            : base(externalId, metadata)
        {
            // Validate constructor arguments before assigning them
            // so an invalid VideoAsset can never be created.
            if (duration.Value <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "Duration must be positive.",
                    nameof(duration));
            }

            if (string.IsNullOrWhiteSpace(resolution))
            {
                throw new ArgumentException(
                    "Resolution is required.",
                    nameof(resolution));
            }

            if (frameRate <= 0)
            {
                throw new ArgumentException(
                    "Frame rate must be positive.",
                    nameof(frameRate));
            }

            if (string.IsNullOrWhiteSpace(codec))
            {
                throw new ArgumentException(
                    "Codec is required.",
                    nameof(codec));
            }

            if (!SupportedCodecs.Contains(codec))
            {
                throw new ArgumentException(
                    $"Codec '{codec}' is not supported.",
                    nameof(codec));
            }

            Duration = duration;
            Resolution = resolution;
            FrameRate = frameRate;
            Codec = codec;
        }
    }
}