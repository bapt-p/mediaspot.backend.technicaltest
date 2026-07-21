using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Commands.CreateTitle
{
    public sealed record CreateTitleCommand(string Name, string? Description, DateOnly? ReleaseDate, TitleType Type) : IRequest<Guid>;
}
