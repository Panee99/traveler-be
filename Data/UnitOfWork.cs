using Data.Repositories;
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

    public IManagerRepository Manager
    {
        get { return _manager ??= new ManagerRepository(_context); }
    }

    public ITourGuideRepository TourGuide
    {
        get { return _tourGuide ??= new TourGuideRepository(_context); }
    }


    // Generic Repository
    private readonly Dictionary<Type, object> _repoCache = new();

    public IRepository<T> Repo<T>() where T : class
    {
        if (_repoCache.TryGetValue(typeof(T), out var repo))
            return (IRepository<T>)repo;

        var newRepo = new Repository<T>(_context);
        _repoCache.Add(typeof(T), newRepo);
        return newRepo;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();
}