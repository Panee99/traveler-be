using Data.EFCore;
using Service.Interfaces;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    public TourService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}