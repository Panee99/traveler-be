using Data.EFCore;

namespace Service.Implementations;

public class BaseService
{
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }
}