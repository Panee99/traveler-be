using Data.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
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

    public int SaveChanges() => _context.SaveChanges();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();
    
    public EntityEntry<T> Entry<T>(T entity) where T : class
    {
        return _context.Entry(entity);
    }
}