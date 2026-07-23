using Mediaspot.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Domain.Transcoding.Events
{
    public sealed record TranscodeJobFailed(Guid JobId) : IDomainEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
