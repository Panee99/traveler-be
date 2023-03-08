using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class TourRepository : Repository<Tour>, ITourRepository
{
    public TourRepository(AppDbContext context) : base(context)
    {
    }
}