using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TravelerDbContext _context;

    private IAccountRepository? _account;
    private ITravellerRepository? _traveller;
    private IManagerRepository? _manager;
    private ITourGuideRepository? _tourGuide;

    public UnitOfWork(TravelerDbContext context)
    {
        _context = context;
    }

    public IAccountRepository Account
    {
        get { return _account ??= new AccountRepository(_context); }
    }

    public ITravellerRepository Traveller
    {
        get { return _traveller ??= new TravellerRepository(_context); }
    }

    public IManagerRepository Manager
    {
        get { return _manager ??= new ManagerRepository(_context); }
    }

    public ITourGuideRepository TourGuide
    {
        get { return _tourGuide ??= new TourGuideRepository(_context); }
    }

    public async Task<int> SaveChanges()
    {
        return await _context.SaveChangesAsync();
    }

    public IDbContextTransaction Transaction()
    {
        return _context.Database.BeginTransaction();
    }
}