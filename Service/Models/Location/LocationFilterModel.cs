namespace Service.Models.Location;

public record LocationFilterModel
(
    int Page,
    int Size,
    string? Name,
    List<Guid>? Tags
);