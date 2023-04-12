using Data.EFCore;
using Microsoft.AspNetCore.Http;
using Service.Models.Auth;
using Shared;

namespace Service.Implementations;

public class BaseService
{
    protected readonly IUnitOfWork UnitOfWork;
    protected AuthUser? AuthUser;

    protected BaseService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected BaseService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        UnitOfWork = unitOfWork;
        AuthUser = (AuthUser)httpContextAccessor.HttpContext?.Items[AppConstants.UserContextKey]!;
    }
}