using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context)
    {
    }
}