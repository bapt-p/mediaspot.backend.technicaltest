using System;
using Mediaspot.Application.Common;
using MediatR;

namespace Mediaspot.Application.Titles.Commands.UpdateTitle
{
    public sealed class UpdateTitleCommandHandler : IRequestHandler<UpdateTitleCommand>
    {
        private readonly ITitleRepository _titleRepository;
        private readonly IUnitOfWork _uow;

        public UpdateTitleCommandHandler(ITitleRepository titleRepository, IUnitOfWork uow)
        {
            _titleRepository = titleRepository;
            _uow = uow;
        }

        public async Task Handle(
            UpdateTitleCommand command,
            CancellationToken cancellationToken)
        {
            // Validate that the target title exist and no other title already uses the requested name
            var title = await _titleRepository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (title is null)
            {
                throw new KeyNotFoundException(
                    $"Title with id '{command.Id}' was not found.");
            }

            var existingTitle = await _titleRepository.GetByNameAsync(
                command.Name,
                cancellationToken);

            // Allow the current title to keep its name, but prevent another title
            // from using the same name.
            if (existingTitle is not null &&
                existingTitle.Id != title.Id)
            {
                throw new InvalidOperationException(
                    $"A title named '{command.Name}' already exists.");
            }

            title.Update(
                    command.Name,
                    command.Description,
                    command.ReleaseDate,
                    command.Type);

            // Commits all changes made during the current operation.
            await _uow.SaveChangesAsync();

        }
    }
}
