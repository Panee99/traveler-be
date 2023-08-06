using Data.EFCore;
using Microsoft.AspNetCore.Http;
using Service.Models.Auth;
using Shared;

namespace Service.Implementations;

public abstract class BaseService
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    protected readonly UnitOfWork UnitOfWork;

    protected AuthUser CurrentUser
    {
        get
        {
            if (_httpContextAccessor is null) throw new Exception("HttpContextAccessor not set on BaseService.");
            var user = (AuthUser?)_httpContextAccessor.HttpContext?.Items[AppConstants.UserContextKey];
            if (user is null) throw new Exception("User not exist in current Context");
            return user;
        }
    }

    protected BaseService(UnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected BaseService(UnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        UnitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }
}