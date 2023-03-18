using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Results;
using Shared.Firebase;
using Shared.Settings;

namespace Service.Implementations;

public class FileUploadUploadService : IFileUploadService
{
    private static readonly StorageClient _storage;
    private readonly CloudStorageSettings _settings;
    private readonly ILogger<FileUploadUploadService> _logger;

    static FileUploadUploadService()
    {
        _storage = CloudStorageHelper.GetStorage();
    }


    public FileUploadUploadService(CloudStorageSettings settings, ILogger<FileUploadUploadService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<Result> Upload(Guid id, string contentType, Stream stream)
    {
        try
        {
            await _storage.UploadObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                contentType,
                stream,
                null,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }

        return Result.Success();
    }
}