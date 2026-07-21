using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Commands.UpdateTitle
{
    public sealed record UpdateTitleCommand(Guid Id, string Name, string? Description, DateOnly ReleaseDate, TitleType Type) : IRequest;
}
