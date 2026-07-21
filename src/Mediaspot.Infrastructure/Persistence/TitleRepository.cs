using Mediaspot.Application.Common;
using Mediaspot.Domain.Titles;
using Microsoft.EntityFrameworkCore;

namespace Mediaspot.Infrastructure.Persistence;
public sealed class TitleRepository(MediaspotDbContext db) : ITitleRepository
{
    public async Task AddTitleAsync(Title title, CancellationToken cancellationToken)
        => await db.Titles.AddAsync(title, cancellationToken);

    public Task<Title?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
   => db.Titles.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);


    public Task<List<Title>> GetAllAsync(CancellationToken cancellationToken)
        => db.Titles.ToListAsync(cancellationToken);

    public Task<Title?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => db.Titles.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
}
