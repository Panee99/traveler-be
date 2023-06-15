using System.Net;
using Data.EFCore;
using Google;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Shared.ExternalServices.CloudStorage;
using Shared.ResultExtensions;
using Shared.Settings;

namespace Service.Implementations;

public class CloudStorageService : BaseService, ICloudStorageService
{
    private static readonly StorageClient Storage;
    private readonly ILogger<CloudStorageService> _logger;
    private readonly CloudStorageSettings _settings;

    static CloudStorageService()
    {
        Storage = CloudStorageHelper.GetStorage();
    }

    public CloudStorageService(UnitOfWork unitOfWork, IOptions<CloudStorageSettings> settings,
        ILogger<CloudStorageService> logger) : base(unitOfWork)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> Upload(Guid id, string contentType, Stream stream)
    {
        try
        {
            await Storage.UploadObjectAsync(
                _settings.Bucket,
                $"{_settings.Folder}/{id}",
                contentType,
                stream,
                null,
                CancellationToken.None);

            return GetMediaLink(id)!;
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
    public string? GetMediaLink(Guid? id)
    {
        // return CloudStorageHelper.GenerateV4UploadSignedUrl(
        //     _settings.Bucket,
        //     _settings.Folder + '/' + id);
        
        // return $"https://firebasestorage.googleapis.com/v0/b/" +
        //        $"{_settings.Bucket}/o/{_settings.Folder}%2F{id}?alt=media";
        
        return id is null
            ? null
            : $"https://storage.googleapis.com/{_settings.Bucket}/{_settings.Folder}/{id}";
    }
}