using Data.Entities;
using Data.Repositories.Implementations;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TravellerContext _context;

        private IAccountRepository _account = null!;
        private ITravellerRepository _traveller = null!;
        private IManagerRepository _manager = null!;
        private ITourGuideRepository _tourGuide = null!;

        public UnitOfWork(TravellerContext context)
        {
            _context = context;
        }

        public IAccountRepository Account
        {
            get { return _account ??= new AccountRepository(_context); }
        }

        public ITravellerRepository Traveller
        {
            get { return _traveller ??= new TravellerRepository(_context); }
        }

        public IManagerRepository Manager
        {
            get { return _manager ??= new ManagerRepository(_context); }
        }

        public ITourGuideRepository TourGuide
        {
            get { return _tourGuide ??= new TourGuideRepository(_context); }
        }

        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public IDbContextTransaction Transaction()
        {
            return _context.Database.BeginTransaction();
        }
    }
}
