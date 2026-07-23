using Mediaspot.Application.Common;
using MediatR;

namespace Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob
{
    public sealed class CompleteTranscodeJobCommandHandler : IRequestHandler<CompleteTranscodeJobCommand>
    {
        private readonly ITranscodeJobRepository _jobs;
        private readonly IUnitOfWork _uow;
        public CompleteTranscodeJobCommandHandler(ITranscodeJobRepository jobs, IUnitOfWork uow)
        {
            _jobs = jobs;
            _uow = uow;
        }

        public async Task Handle(CompleteTranscodeJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobs.GetByIdAsync(request.JobId, cancellationToken);

            if (job is null)
            {
                throw new KeyNotFoundException(
                    $"Transcode job '{request.JobId}' was not found.");
            }

            job.MarkSucceeded();

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
