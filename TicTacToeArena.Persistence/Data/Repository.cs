using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Persistence.Data;

namespace TicTacToeArena.Domain.Common;

public sealed class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly AppDbContext _db;
    private readonly DbSet<TEntity> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
      return await _set.FirstOrDefaultAsync(x => EqualityComparer<TKey>.Default.Equals(x.Id, id), ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = _set;

        if (queryShaper is not null)
            q = queryShaper(q);

        if (asNoTracking)
            q = q.AsNoTracking();

        return await q.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = _set;

        if (predicate is not null)
            q = q.Where(predicate);

        if (queryShaper is not null)
            q = queryShaper(q);

        if (asNoTracking)
            q = q.AsNoTracking();

        return await q.ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await _set.AsNoTracking().AnyAsync(predicate, ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        await _set.AddRangeAsync(entities, ct);
    }

    public void Update(TEntity entity)
    {
        _set.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _set.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _set.RemoveRange(entities);
    }

}
