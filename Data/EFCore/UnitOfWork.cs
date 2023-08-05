using Data.EFCore.Repositories;
using Data.Entities;
using Data.Entities.Activities;

namespace Data.EFCore;

public class UnitOfWork : UnitOfWorkBase
{
    public IRepository<User> Users => Repo<User>();
    public IRepository<Attachment> Attachments => Repo<Attachment>();
    public IRepository<AttendanceDetail> AttendanceDetails => Repo<AttendanceDetail>();
    public IRepository<Schedule> Schedules => Repo<Schedule>();
    public IRepository<Manager> Managers => Repo<Manager>();
    public IRepository<Tour> Tours => Repo<Tour>();
    public IRepository<Trip> Trips => Repo<Trip>();
    public IRepository<TourImage> TourCarousel => Repo<TourImage>();
    public IRepository<TourGroup> TourGroups => Repo<TourGroup>();
    public IRepository<TourGuide> TourGuides => Repo<TourGuide>();
    public IRepository<Traveler> Travelers => Repo<Traveler>();
    public IRepository<TravelerInTourGroup> TravelersInTourGroups => Repo<TravelerInTourGroup>();
    public IRepository<FcmToken> FcmTokens => Repo<FcmToken>();
    public IRepository<Notification> Notifications => Repo<Notification>();
    public IRepository<AttendanceActivity> AttendanceActivities => Repo<AttendanceActivity>();
    public IRepository<AttendanceItem> AttendanceItems => Repo<AttendanceItem>();
    public IRepository<CustomActivity> CustomActivities => Repo<CustomActivity>();
    public IRepository<CheckInActivity> CheckInActivities => Repo<CheckInActivity>();
    public IRepository<WeatherAlert> WeatherAlerts => Repo<WeatherAlert>();
    public IRepository<WeatherForecast> WeatherForecasts => Repo<WeatherForecast>();
    
    public UnitOfWork(AppDbContext context) : base(context)
    {
    }
}