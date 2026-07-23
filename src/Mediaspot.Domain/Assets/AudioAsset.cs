using Mediaspot.Domain.Assets.ValueObjects;

namespace Mediaspot.Domain.Assets
{
    public sealed class AudioAsset : Asset
    {
        public Duration Duration { get; private set; } = default!;
        public int Bitrate { get; private set; }
        public int SampleRate { get; private set; }
        public int Channels { get; private set; }

        // Required by EF Core.
        private AudioAsset()
            : base()
        {
        }

        public AudioAsset(
            string externalId,
            Metadata metadata,
            Duration duration,
            int bitrate,
            int sampleRate,
            int channels)
            : base(externalId, metadata)
        {
            // Validate arguments before assigning them so an invalid
            // AudioAsset can never be created.
            if (duration.Value <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "Duration must be positive.",
                    nameof(duration));
            }

            if (bitrate <= 0)
            {
                throw new ArgumentException(
                    "Bitrate must be positive.",
                    nameof(bitrate));
            }

            if (sampleRate <= 0)
            {
                throw new ArgumentException(
                    "Sample rate must be positive.",
                    nameof(sampleRate));
            }

            if (channels <= 0)
            {
                throw new ArgumentException(
                    "Channels must be positive.",
                    nameof(channels));
            }

            Duration = duration;
            Bitrate = bitrate;
            SampleRate = sampleRate;
            Channels = channels;
        }
    }
}