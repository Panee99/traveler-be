using Data.EFCore.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.EFCore;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    // Generic Repository
    private readonly Dictionary<Type, object> _repoCache = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repo<T>() where T : class
    {
        if (_repoCache.TryGetValue(typeof(T), out var repo))
            return (IRepository<T>)repo;

        var newRepo = new Repository<T>(_context);
        _repoCache.Add(typeof(T), newRepo);
        return newRepo;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public IDbContextTransaction BeginTransaction()
    {
        return _context.Database.BeginTransaction();
    }
}