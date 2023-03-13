using Data.Repositories;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public interface IUnitOfWork
{
    public IAccountRepository Account { get; }
    public IManagerRepository Manager { get; }
    public ITourGuideRepository TourGuide { get; }
    public ITravelerRepository Traveler { get; }

    IRepository<T> Repo<T>() where T : class;
    Task<int> SaveChangesAsync();
    IDbContextTransaction BeginTransaction();
}