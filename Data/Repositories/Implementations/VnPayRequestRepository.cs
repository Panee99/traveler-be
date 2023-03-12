using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class VnPayRequestRepository : Repository<VnPayRequest>, IVnPayRequestRepository
{
    public VnPayRequestRepository(AppDbContext context) : base(context)
    {
    }
}