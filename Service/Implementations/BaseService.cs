using Data.EFCore;

namespace Service.Implementations;

public class BaseService
{
    protected readonly IUnitOfWork _unitOfWork;

    protected BaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}