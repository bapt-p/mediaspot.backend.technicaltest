using Mediaspot.Domain.Titles;

namespace Mediaspot.Application.Common
{
    public interface ITitleRepository
    {
        Task AddTitleAsync(Title title, CancellationToken cancellationToken);
        Task<Title?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Title>> GetAllAsync(CancellationToken cancellationToken);
        Task<Title?> GetByNameAsync(string name,CancellationToken cancellationToken);
    }
}
