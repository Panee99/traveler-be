using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ICloudStorageService
{
    Task<Result<string>> Upload(Guid id, string contentType, Stream stream);

    Task<Result> Delete(Guid id);

    string? GetMediaLink(Guid? id);
}