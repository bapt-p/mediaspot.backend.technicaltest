using Mediaspot.Application.Common;
using Mediaspot.Domain.Titles;
using MediatR;

namespace Mediaspot.Application.Titles.Commands.CreateTitle
{
    public sealed class CreateTitleCommandHandler : IRequestHandler<CreateTitleCommand, Guid>
    {
        private readonly ITitleRepository _titleRepository;
        private readonly IUnitOfWork _uow;

        public CreateTitleCommandHandler(ITitleRepository titleRepository, IUnitOfWork uow)
        {
            _titleRepository = titleRepository;
            _uow = uow;
        }

        public async Task<Guid> Handle(
            CreateTitleCommand command,
            CancellationToken cancellationToken)
        {
            var existingTitle = await _titleRepository.GetByNameAsync(
            command.Name,
            cancellationToken);

            // Title names must be unique across the system.
            if (existingTitle is not null)
            {
                throw new InvalidOperationException(
                    $"A title named '{command.Name}' already exists.");
            }

            var title = Title.Create(
                command.Name,
                command.Description,
                command.ReleaseDate,
                command.Type);

            await _titleRepository.AddTitleAsync(title, cancellationToken);

            // Commits all changes made during the current operation.
            await _uow.SaveChangesAsync(cancellationToken);

            return title.Id;
        }
    }
}
