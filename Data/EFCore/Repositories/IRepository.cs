using System.Linq.Expressions;

namespace Data.EFCore.Repositories;

public interface IRepository<T> where T : class
{
    T Add(T entity);

    T Update(T entity);

    T Remove(T entity);

    void AddRange(IEnumerable<T> entities);

    void UpdateRange(IEnumerable<T> entities);

    void RemoveRange(IEnumerable<T> entities);

    IQueryable<T> TrackingQuery();

    IQueryable<T> Query();

    ValueTask<T?> FindAsync(params object[] keyValues);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}