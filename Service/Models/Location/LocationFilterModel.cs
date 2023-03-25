// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global

namespace Service.Models.Location;

public record LocationFilterModel : PagingFilterModel
{
    public string? Name;
    public List<Guid>? Tags;
}