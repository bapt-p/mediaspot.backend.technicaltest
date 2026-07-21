using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Queries.GetTitles
{
    public sealed record GetTitlesQuery : IRequest<List<Title>>;
}
