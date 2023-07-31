using System.Net;
using Google;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Settings;
using Shared.ExternalServices.CloudStorage;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class CloudStorageService : ICloudStorageService
{
    private static readonly StorageClient Storage;
    private readonly ILogger<CloudStorageService> _logger;
    private readonly CloudStorageSettings _settings;

    static CloudStorageService()
    {
        Storage = CloudStorageHelper.GetStorage();
    }

    public CloudStorageService(IOptions<CloudStorageSettings> settings,
        ILogger<CloudStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> Upload(string fileName, string contentType, Stream stream)
    {
        try
        {
            await Storage.UploadObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{fileName}",
                contentType,
                stream,
                null,
                CancellationToken.None);

            return GetMediaLink(fileName)!;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    // Delete an object, IsSuccess if deleted successfully or not found
    public async Task<Result> Delete(string fileName)
    {
        try
        {
            await Storage.DeleteObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{fileName}",
                null,
                CancellationToken.None
            );

            return Result.Success();
        }
        catch (GoogleApiException e)
        {
            if ((int)e.HttpStatusCode == (int)HttpStatusCode.NotFound)
                return Result.Success();

            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    // Object url
    public string? GetMediaLink(string? fileName)
    {
        return fileName is null
            ? null
            : $"https://storage.googleapis.com/{_settings.Bucket}/{_settings.Folder}/{fileName}";
    }
}