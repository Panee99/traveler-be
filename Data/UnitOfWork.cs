using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IAccountRepository? _account;
    private ITravelerRepository? _traveler;
    private IManagerRepository? _manager;
    private ITourGuideRepository? _tourGuide;
    private ITourRepository? _tour;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IAccountRepository Account
    {
        get { return _account ??= new AccountRepository(_context); }
    }

    public ITravelerRepository Traveler
    {
        get { return _traveler ??= new TravelerRepository(_context); }
    }

    public ITourRepository Tour
    {
        get { return _tour ??= new TourRepository(_context); }
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

    public IDbContextTransaction BeginTransaction()
    {
        return _context.Database.BeginTransaction();
    }
}