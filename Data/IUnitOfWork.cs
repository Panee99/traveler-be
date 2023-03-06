using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public interface IUnitOfWork
{
    public IAccountRepository Account { get; }
    public IManagerRepository Manager { get; }
    public ITourGuideRepository TourGuide { get; }
    public ITravelerRepository Traveler { get; }

    Task<int> SaveChanges();
    IDbContextTransaction Transaction();
}