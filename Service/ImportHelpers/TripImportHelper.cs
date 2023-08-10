using System.Data;
using System.Globalization;
using System.IO.Compression;
using Data.Enums;
using ExcelDataReader;

namespace Service.ImportHelpers;

public class TripImportHelper
{
    /// <summary>
    /// Extract TripModel from zip archive
    /// </summary>
    public static TripModel ReadTripArchive(Stream zipData)
    {
        var zipArchive = new ZipArchive(zipData);
        return _readTripExcel(zipArchive);
    }

    /// <summary>
    /// Read Trip from excel file
    /// </summary>
    private static TripModel _readTripExcel(ZipArchive zipArchive)
    {
        var tripExcelFile = zipArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("Trip.xlsx"));
        if (tripExcelFile is null) throw new Exception("Trip.xlsx not found");

        var tripExcelData = new MemoryStream();
        tripExcelFile.Open().CopyTo(tripExcelData);

        // Create excel reader
        using var reader = ExcelReaderFactory.CreateReader(tripExcelData);

        // Read Trip
        reader.Read(); // skip header
        reader.Read(); // trip data
        var trip = new TripModel()
        {
            TourId = Guid.Parse(reader.GetString(0)),
            StartTime = reader.GetDateTime(1),
            EndTime = reader.GetDateTime(2),
        };

        // Read Groups - Users
        reader.NextResult();
        trip.TourGroups = ReadTourGroupModels(reader);

        return trip;
    }

    /// <summary>
    /// Read TourGroup includes Users from excel file
    /// </summary>
    private static List<TourGroupModel> ReadTourGroupModels(IDataReader reader)
    {
        var userTuples = new List<(int GroupNo, UserModel User)>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            var userModel = new UserModel
            {
                Phone = reader.GetDouble(0).ToString(CultureInfo.InvariantCulture),
                Email = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Gender = Enum.Parse<Gender>(reader.GetString(4)),
                Role = Enum.Parse<UserRole>(reader.GetString(5))
            };

            var groupNo = (int)reader.GetDouble(6);

            userTuples.Add((groupNo, userModel));
        }

        var groups = userTuples
            .GroupBy(userTuple => userTuple.GroupNo)
            .Select(grouping => (
                GroupNo: grouping.Key,
                Users: grouping.Select(userTuple => userTuple.User).ToList())
            )
            .Select(groupTuple =>
            {
                var group = new TourGroupModel()
                {
                    GroupNo = groupTuple.GroupNo
                };

                // User Role must be TourGuide or Traveler
                if (groupTuple.Users.Exists(user =>
                        user.Role != UserRole.TourGuide && user.Role != UserRole.Traveler))
                {
                    throw new Exception("User Role can be TourGuide and Traveler only");
                }

                // Each group have 1 TourGuide
                var tourGuides = groupTuple.Users.Where(user => user.Role is UserRole.TourGuide).ToList();
                if (tourGuides.Count != 1) throw new Exception("Each group must have 1 TourGuide");

                // Each group have at least 1 Traveler
                var travelers = groupTuple.Users.Where(user => user.Role is UserRole.Traveler).ToList();
                if (!travelers.Any()) throw new Exception("Each group must have at least 1 Traveler");

                // Return
                group.TourGuide = tourGuides.FirstOrDefault()!;
                group.Travelers = travelers;
                return group;
            });

        return groups.ToList();
    }

    public sealed class TripModel
    {
        public Guid TourId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ICollection<TourGroupModel> TourGroups { get; set; } = new List<TourGroupModel>();
    }

    public sealed class TourGroupModel
    {
        public int GroupNo { get; set; }
        public UserModel TourGuide { get; set; } = null!;
        public ICollection<UserModel> Travelers { get; set; } = new List<UserModel>();
    }

    public class UserModel
    {
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }
    }
}