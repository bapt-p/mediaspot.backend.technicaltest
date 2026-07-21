using Mediaspot.Application.Common;
using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Queries.GetTitleById
{
    public sealed class GetTitleByIdQueryHandler : IRequestHandler<GetTitleByIdQuery, Title?>
    {
        private readonly ITitleRepository _titleRepository;

        public GetTitleByIdQueryHandler(ITitleRepository titleRepository)
        {
            _titleRepository = titleRepository;
        }

        public async Task<Title?> Handle(
            GetTitleByIdQuery query,
            CancellationToken cancellationToken)
        {
            return await _titleRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}
