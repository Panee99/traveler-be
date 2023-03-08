using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _entities;

    protected Repository(AppDbContext context)
    {
        _entities = context.Set<T>();
    }

    public async Task<T> Add(T entity)
    {
        var result = await _entities.AddAsync(entity);
        return result.Entity;
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _entities.AddRangeAsync(entities);
    }

    public IQueryable<T> GetAll()
    {
        return _entities;
    }

    public IQueryable<T> GetMany(Expression<Func<T, bool>> predicate)
    {
        return _entities.Where(predicate);
    }

    public IQueryable<T> SkipAndTake(int skip, int take)
    {
        return _entities.Skip(skip).Take(take);
    }

    public IQueryable<T> Distinct(Expression<Func<T, bool>> predicate)
    {
        return _entities.Where(predicate).Distinct();
    }

    public int Count()
    {
        return _entities.Count();
    }

    public void Remove(T entity)
    {
        _entities.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _entities.RemoveRange(entities);
    }

    public void Update(T entity)
    {
        _entities.Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _entities.UpdateRange(entities);
    }

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _entities.FirstOrDefault(predicate) ?? null!;
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _entities.FirstOrDefaultAsync(predicate) ?? null!;
    }

    public bool Contains(Expression<Func<T, bool>> predicate)
    {
        return _entities.Where(predicate).ToList().Count > 0;
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        return _entities.Any(predicate);
    }
}