using Mediaspot.Domain.Transcoding;
using Mediaspot.Domain.Transcoding.Events;
using Shouldly;

namespace Mediaspot.UnitTests
{
    public class TranscodeJobTests
    {
        [Fact]
        public void Created_Job_Should_Be_Pending()
        {
            var assetId = Guid.NewGuid();
            var mediaFileId = Guid.NewGuid();
            var preset = "720p";

            var transcodeJob = new TranscodeJob(assetId, mediaFileId, preset);

            transcodeJob.Status.ShouldBe(TranscodeStatus.Pending);
        }

        [Fact]
        public void Created_Job_Should_Initialize_Timestamps()
        {
            var transcodeJob = new TranscodeJob(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "720p");

            transcodeJob.CreatedAt.ShouldNotBe(default);
            transcodeJob.UpdatedAt.ShouldNotBe(default);

            transcodeJob.UpdatedAt.ShouldBe(transcodeJob.CreatedAt);
        }

        [Fact]
        public void MarkRunning_Should_Set_Status_To_Running_And_Raise_Started_Event()
        {
            var assetId = Guid.NewGuid();
            var mediaFileId = Guid.NewGuid();
            var preset = "720p";

            var transcodeJob = new TranscodeJob(assetId, mediaFileId, preset);

            transcodeJob.MarkRunning();

            transcodeJob.Status.ShouldBe(TranscodeStatus.Running);

            transcodeJob.DomainEvents.Count.ShouldBe(1);

            transcodeJob.DomainEvents.ShouldContain(domainEvent => domainEvent is TranscodeJobStarted);
        }

        [Fact]
        public void MarkSucceeded_Should_Set_Status_To_Succeeded_And_Raise_Completed_Event()
        {
            var assetId = Guid.NewGuid();
            var mediaFileId = Guid.NewGuid();
            var preset = "720p";

            var transcodeJob = new TranscodeJob(assetId, mediaFileId, preset);

            transcodeJob.MarkRunning();

            transcodeJob.MarkSucceeded();

            transcodeJob.Status.ShouldBe(TranscodeStatus.Succeeded);

            transcodeJob.DomainEvents
                .OfType<TranscodeJobCompleted>()
                .Count()
                .ShouldBe(1);
        }

        [Fact]
        public void MarkFailed_Should_Set_Status_To_Succeded_And_Raise_Failed_Event()
        {
            var assetId = Guid.NewGuid();
            var mediaFileId = Guid.NewGuid();
            var preset = "720p";

            var transcodeJob = new TranscodeJob(assetId, mediaFileId, preset);

            transcodeJob.MarkRunning();
            transcodeJob.MarkFailed();

            transcodeJob.Status.ShouldBe(TranscodeStatus.Failed);

            transcodeJob.DomainEvents
                .OfType<TranscodeJobFailed>()
                .Count()
                .ShouldBe(1);
        }

        [Fact]
        public void MarkSucceeded_Should_Throw_When_Job_Is_Not_Running()
        {
            var transcodeJob = new TranscodeJob(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "720p");

            Should.Throw<InvalidOperationException>(
                () => transcodeJob.MarkSucceeded());

            transcodeJob.Status.ShouldBe(TranscodeStatus.Pending);

            transcodeJob.DomainEvents.ShouldNotContain(
                domainEvent => domainEvent is TranscodeJobCompleted);
        }
    }
}
