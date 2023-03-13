using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class VnPayResponseRepository : Repository<VnPayResponse>, IVnPayResponseRepository
{
    public VnPayResponseRepository(AppDbContext context) : base(context)
    {
    }
}