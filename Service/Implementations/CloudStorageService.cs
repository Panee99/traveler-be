using System.Net;
using Google;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Shared.ExternalServices.CloudStorage;
using Shared.ResultExtensions;
using Shared.Settings;

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

    public CloudStorageService(IOptions<CloudStorageSettings> settings, ILogger<CloudStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> Upload(Guid id, string contentType, Stream stream)
    {
        Console.WriteLine("Cloud");
        try
        {
            await Storage.UploadObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                contentType,
                stream,
                null,
                CancellationToken.None);

            return CloudStorageHelper.GenerateV4UploadSignedUrl(
                _settings.Bucket,
                $"{_settings.Folder}/{id}");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    // Delete an object, IsSuccess if deleted successfully or not found
    public async Task<Result> Delete(Guid id)
    {
        Console.WriteLine("Cloud");
        try
        {
            await Storage.DeleteObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
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
    public string GetMediaLink(Guid id)
    {
        return CloudStorageHelper.GenerateV4UploadSignedUrl(
            _settings.Bucket,
            _settings.Folder + '/' + id);
    }
}