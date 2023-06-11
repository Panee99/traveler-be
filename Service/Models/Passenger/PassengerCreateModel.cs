namespace Service.Models.Passenger;

public record PassengerCreateModel(
    string Name,
    string? Phone,
    string? Address,
    string? Gender,
    string? Country,
    string? Passport
);