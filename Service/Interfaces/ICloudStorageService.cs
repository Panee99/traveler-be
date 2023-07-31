using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ICloudStorageService
{
    Task<Result<string>> Upload(string fileName, string contentType, Stream stream);

    Task<Result> Delete(string fileName);

    string? GetMediaLink(string? fileName);
}