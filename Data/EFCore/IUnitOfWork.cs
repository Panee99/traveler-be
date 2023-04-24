using Data.EFCore.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.EFCore;

public interface IUnitOfWork
{
    IRepository<T> Repo<T>() where T : class;

    Task<int> SaveChangesAsync();

    IDbContextTransaction BeginTransaction();

    EntityEntry<T> Attach<T>(T entity) where T : class;
}