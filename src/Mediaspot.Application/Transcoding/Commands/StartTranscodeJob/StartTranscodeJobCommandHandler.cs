using Mediaspot.Application.Assets.Commands.Archive;
using Mediaspot.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Transcoding.Commands.StartTranscodeJob
{
    public sealed class StartTranscodeJobCommandHandler(ITranscodeJobRepository jobs, IUnitOfWork uow)
    : IRequestHandler<StartTranscodeJobCommand>
    {
        public Task Handle(StartTranscodeJobCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
