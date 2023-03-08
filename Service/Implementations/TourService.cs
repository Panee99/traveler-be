using Data;
using Service.Interfaces;

namespace Service.Implementations;

public class TourService : ITourService
{
    private readonly IUnitOfWork _unitOfWork;

    public TourService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}