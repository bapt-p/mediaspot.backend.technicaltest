using Mediaspot.Application.Assets.Commands.Archive;
using Mediaspot.Application.Common;
using Mediaspot.Domain.Transcoding;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Transcoding.Commands.StartTranscodeJob
{
    public sealed class StartTranscodeJobCommandHandler
    : IRequestHandler<StartTranscodeJobCommand>
    {
        private readonly ITranscodeJobRepository _jobs;
        private readonly IUnitOfWork _uow;

        public StartTranscodeJobCommandHandler(ITranscodeJobRepository jobs, IUnitOfWork uow)
        {
            _jobs = jobs;
            _uow = uow;
        }

        public async Task Handle(StartTranscodeJobCommand request,CancellationToken cancellationToken)
        {
            var job = await _jobs.GetByIdAsync(
                request.JobId,
                cancellationToken);

            if (job is null)
            {
                throw new KeyNotFoundException(
                    $"Transcode job '{request.JobId}' was not found.");
            }

            job.MarkRunning();

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
