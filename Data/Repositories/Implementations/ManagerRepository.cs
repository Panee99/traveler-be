using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class ManagerRepository : Repository<Manager>, IManagerRepository
{
    public ManagerRepository(AppDbContext context) : base(context)
    {
    }
}