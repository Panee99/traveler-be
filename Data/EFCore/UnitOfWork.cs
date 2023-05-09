using Data.EFCore.Repositories;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Helpers;

namespace Data.EFCore;

public class UnitOfWork : UnitOfWorkBase
{
    public IRepository<Account> Accounts => Repo<Account>();
    public IRepository<Attachment> Attachments => Repo<Attachment>();
    public IRepository<Booking> Bookings => Repo<Booking>();
    public IRepository<Schedule> Schedules => Repo<Schedule>();
    public IRepository<TourFlow> TourFlows => Repo<TourFlow>();
    public IRepository<Manager> Managers => Repo<Manager>();
    public IRepository<Ticket> Tickets => Repo<Ticket>();
    public IRepository<Tour> Tours => Repo<Tour>();
    public IRepository<TourImage> TourCarousel => Repo<TourImage>();
    public IRepository<TourGroup> TourGroups => Repo<TourGroup>();
    public IRepository<TourGuide> TourGuides => Repo<TourGuide>();
    public IRepository<Transaction> Transactions => Repo<Transaction>();
    public IRepository<Traveler> Travelers => Repo<Traveler>();
    public IRepository<TravelerInTour> TravelersInTours => Repo<TravelerInTour>();
    public IRepository<VnPayRequest> VnPayRequests => Repo<VnPayRequest>();
    public IRepository<VnPayResponse> VnPayResponses => Repo<VnPayResponse>();
    public IRepository<IncurredCost> IncurredCosts => Repo<IncurredCost>();

    public UnitOfWork(AppDbContext context) : base(context)
    {
    }
}