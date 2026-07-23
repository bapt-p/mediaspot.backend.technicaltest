using Mediaspot.Application.Common;
using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using MediatR;

namespace Mediaspot.Application.Assets.Commands.Create.CreateVideoAsset;

public sealed class CreateVideoAssetHandler(
    IAssetRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateVideoAssetCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateVideoAssetCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByExternalIdAsync(
            request.ExternalId,
            cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Asset with ExternalId '{request.ExternalId}' already exists.");
        }

        var metadata = new Metadata(
            request.Title,
            request.Description,
            request.Language);

        var asset = new VideoAsset(
            request.ExternalId,
            metadata,
            request.Duration,
            request.Resolution,
            request.FrameRate,
            request.Codec);

        await repository.AddAsync(
            asset,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return asset.Id;
    }
}