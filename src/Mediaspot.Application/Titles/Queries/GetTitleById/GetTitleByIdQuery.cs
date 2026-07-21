using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Queries.GetTitleById
{
    public sealed record GetTitleByIdQuery(Guid Id) : IRequest<Title?>;
}
