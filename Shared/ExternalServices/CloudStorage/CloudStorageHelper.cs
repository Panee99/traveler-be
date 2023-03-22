using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Shared.ExternalServices.CloudStorage;

// Create Cloud Storage needed instances
public static class CloudStorageHelper
{
    private static readonly StorageClient _storage;
    private static readonly UrlSigner _urlSigner;

    static CloudStorageHelper()
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var credentialPath = Path.Combine(projectPath, "ExternalServices", "CloudStorage", "cloud-storage.json");
        var credential = GoogleCredential.FromFile(credentialPath);

        // Storage
        _storage = StorageClient.Create(credential);

        // Url Signer
        _urlSigner = UrlSigner.FromCredential(credential);
    }

    public static StorageClient GetStorage()
    {
        return _storage;
    }

    // Generate signed cloud storage object url 
    public static string GenerateV4UploadSignedUrl(string bucketName, string objectName)
    {
        var options = UrlSigner.Options.FromDuration(TimeSpan.FromHours(24));

        var template = UrlSigner.RequestTemplate
            .FromBucket(bucketName)
            .WithObjectName(objectName)
            .WithHttpMethod(HttpMethod.Get);

        return _urlSigner.Sign(template, options);
    }
}