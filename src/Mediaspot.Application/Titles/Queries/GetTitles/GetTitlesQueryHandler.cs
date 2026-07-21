using Mediaspot.Application.Common;
using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Queries.GetTitles;

public sealed class GetTitlesQueryHandler : IRequestHandler<GetTitlesQuery, List<Title>>
{
    private readonly ITitleRepository _titleRepository;

    public GetTitlesQueryHandler(ITitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task<List<Title>> Handle(
        GetTitlesQuery query,
        CancellationToken cancellationToken)
    {
        return await _titleRepository.GetAllAsync(cancellationToken);
    }
}
