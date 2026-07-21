using Mediaspot.Domain.Titles;

namespace Mediaspot.Api.DTOs;

public sealed record UpdateTitleDto(string Name, string? Description, DateOnly ReleaseDate, TitleType Type);