using System.Linq.Expressions;

namespace Data.Repositories;

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

    T? FirstOrDefault(Expression<Func<T, bool>> predicate);

    bool Any(Expression<Func<T, bool>> predicate);
}