using Data.EFCore.Repositories;
using Data.Entities;

namespace Data.EFCore;

public class UnitOfWork : UnitOfWorkBase
{
    public IRepository<User> Users => Repo<User>();
    public IRepository<Attachment> Attachments => Repo<Attachment>();
    public IRepository<AttendanceEvent> AttendanceEvents => Repo<AttendanceEvent>();
    public IRepository<Attendance> Attendances => Repo<Attendance>();
    public IRepository<Schedule> Schedules => Repo<Schedule>();
    public IRepository<Admin> Admins => Repo<Admin>();
    public IRepository<Tour> Tours => Repo<Tour>();
    public IRepository<Trip> Trips => Repo<Trip>();
    public IRepository<TourImage> TourCarousel => Repo<TourImage>();
    public IRepository<TourGroup> TourGroups => Repo<TourGroup>();
    public IRepository<TourGuide> TourGuides => Repo<TourGuide>();
    public IRepository<Traveler> Travelers => Repo<Traveler>();
    public IRepository<TravelerInTourGroup> TravelersInTourGroups => Repo<TravelerInTourGroup>();
    public IRepository<IncurredCost> IncurredCosts => Repo<IncurredCost>();
    public IRepository<FcmToken> FcmTokens => Repo<FcmToken>();
    public IRepository<Notification> Notifications => Repo<Notification>(); 
    
    public UnitOfWork(AppDbContext context) : base(context)
    {
    }
}