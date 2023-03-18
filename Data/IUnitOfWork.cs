using Data.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public interface IUnitOfWork
{
    IRepository<T> Repo<T>() where T : class;
    
    int SaveChanges();
    
    Task<int> SaveChangesAsync();
    
    IDbContextTransaction BeginTransaction();
    
    EntityEntry<T> Entry<T>(T entity) where T : class;
}