using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Transcoding.Commands.CompleteTranscodeJob
{
    public record CompleteTranscodeJobCommand(Guid JobId) : IRequest;
}
