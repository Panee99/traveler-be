using Data.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Helpers;

namespace Data.EFCore;

public class UnitOfWorkBase
{
    /// <summary>
    /// Cache created IRepository instances
    /// </summary>
    private readonly Dictionary<Type, object> _repoCache = new();

    private readonly AppDbContext _context;

    protected UnitOfWorkBase(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Save changes
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        _generateValues();
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Begin a Transaction
    /// </summary>
    public IDbContextTransaction BeginTransaction()
    {
        return _context.Database.BeginTransaction();
    }

    /// <summary>
    /// Attach an entity to DbContext
    /// </summary>
    public EntityEntry<T> Attach<T>(T entity) where T : class
    {
        return _context.Attach(entity);
    }

    /// <summary>
    /// Get an IRepository instance
    /// </summary>
    protected IRepository<T> Repo<T>() where T : class
    {
        if (_repoCache.TryGetValue(typeof(T), out var repo))
            return (IRepository<T>)repo;

        var newRepo = new Repository<T>(_context);
        _repoCache.Add(typeof(T), newRepo);
        return newRepo;
    }

    /// <summary>
    /// Generate default values
    /// </summary>
    private void _generateValues()
    {
        var tracker = _context.ChangeTracker;
        if (!tracker.HasChanges()) return;

        var entities = tracker.Entries();
        var addedEntities = entities.Where(e => e.State == EntityState.Added);

        foreach (var entry in addedEntities)
        {
            if (entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
                entry.Property("CreatedAt").CurrentValue = DateTimeHelper.VnNow();
        }
    }
}