using System.Net;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Results;
using Shared.Firebase;
using Shared.Settings;

namespace Service.Implementations;

public class CloudStorageService : ICloudStorageService
{
    private static readonly StorageClient _storage;

    private readonly CloudStorageSettings _settings;
    private readonly ILogger<CloudStorageService> _logger;

    static CloudStorageService()
    {
        _storage = CloudStorageHelper.GetStorage();
    }

    public CloudStorageService(IOptions<CloudStorageSettings> settings, ILogger<CloudStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> Upload(Guid id, string contentType, Stream stream)
    {
        try
        {
            var obj = await _storage.UploadObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                contentType,
                stream,
                null,
                CancellationToken.None);

            return obj.MediaLink;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    public async Task<Result> Delete(Guid id)
    {
        try
        {
            await _storage.DeleteObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                null,
                CancellationToken.None
            );

            return Result.Success();
        }
        catch (Google.GoogleApiException e)
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

    public async Task<Result<string>> GetMediaLink(Guid id)
    {
        try
        {
            var obj = await _storage.GetObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                new GetObjectOptions() { Projection = Projection.NoAcl },
                CancellationToken.None);

            return obj.MediaLink;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }
}