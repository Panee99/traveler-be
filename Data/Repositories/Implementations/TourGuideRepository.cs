using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class TourGuideRepository : Repository<TourGuide>, ITourGuideRepository
{
    public TourGuideRepository(AppDbContext context) : base(context)
    {
    }
}