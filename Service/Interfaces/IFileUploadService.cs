using Service.Results;

namespace Service.Interfaces;

public interface IFileUploadService
{
    Task<Result> Upload(Guid id, string contentType, Stream stream);
}