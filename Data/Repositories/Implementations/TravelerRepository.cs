using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class TravelerRepository : Repository<Traveler>, ITravelerRepository
{
    public TravelerRepository(TravelerDbContext context) : base(context)
    {
    }
}