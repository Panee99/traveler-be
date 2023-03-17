using Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public interface IUnitOfWork
{
    IRepository<T> Repo<T>() where T : class;
    int SaveChanges();
    Task<int> SaveChangesAsync();
    IDbContextTransaction BeginTransaction();
}