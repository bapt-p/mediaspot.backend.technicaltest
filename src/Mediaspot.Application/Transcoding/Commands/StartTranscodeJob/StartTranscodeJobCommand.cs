using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Transcoding.Commands.StartTranscodeJob
{
    public sealed record StartTranscodeJobCommand(Guid JobId) : IRequest;
}
