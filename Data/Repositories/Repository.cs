using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _entities;

    public Repository(AppDbContext context)
    {
        _entities = context.Set<T>();
    }

    public T Add(T entity)
    {
        return _entities.Add(entity).Entity;
    }

    public T Update(T entity)
    {
        return _entities.Update(entity).Entity;
    }

    public T Remove(T entity)
    {
        return _entities.Remove(entity).Entity;
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _entities.AddRangeAsync(entities);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _entities.UpdateRange(entities);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _entities.RemoveRange(entities);
    }

    public IQueryable<T> Query()
    {
        return _entities.AsNoTracking();
    }

    public IQueryable<T> TrackingQuery()
    {
        return _entities.AsQueryable();
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        return _entities.Any(predicate);
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _entities.AsNoTracking().FirstOrDefault(predicate);
    }
}