using Data;

namespace Service;

public class BaseService
{
    protected readonly IUnitOfWork _unitOfWork;

    protected BaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}