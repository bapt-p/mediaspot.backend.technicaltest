using Mediaspot.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Transcoding.Commands.FailTranscodeJob
{
    public sealed class FailTranscodeJobCommandHandler : IRequestHandler<FailTranscodeJobCommand>
    {
        private readonly ITranscodeJobRepository _jobs;
        private readonly IUnitOfWork _uow;
        public FailTranscodeJobCommandHandler(ITranscodeJobRepository jobs, IUnitOfWork uow )
        {
            _jobs = jobs;
            _uow = uow;
        }

        public async Task Handle(FailTranscodeJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobs.GetByIdAsync( request.JobId, cancellationToken );

            if (job is null)
            {
                throw new KeyNotFoundException(
                    $"Transcode job '{request.JobId}' was not found.");
            }

            job.MarkFailed();

            await _uow.SaveChangesAsync(cancellationToken);

        }
    }
}
