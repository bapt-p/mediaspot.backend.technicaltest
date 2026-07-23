using Mediaspot.Domain.Assets.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediaspot.Application.Assets.Commands.Create.CreateVideoAsset
{
    public sealed record CreateVideoAssetCommand(string ExternalId, string Title, string? Description, string? Language, Duration Duration, string Resolution, decimal FrameRate, string Codec) : IRequest<Guid>;
}
