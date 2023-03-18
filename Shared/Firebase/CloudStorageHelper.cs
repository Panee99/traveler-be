using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Shared.Firebase;

public static class CloudStorageHelper
{
    private static readonly StorageClient _storage;

    static CloudStorageHelper()
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var credentialPath = Path.Combine(projectPath, "Firebase", "firebase-admin-sdk.json");
        _storage = StorageClient.Create(GoogleCredential.FromFile(credentialPath));
    }

    public static StorageClient GetStorage()
    {
        return _storage;
    }
}