using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace Shared.ExternalServices.Firebase;

public static class FirestoreHelper
{
    public static FirestoreDb GetInstance()
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var credentialPath = Path.Combine(projectPath, "ExternalServices", "Firebase", "firebase-admin-sdk.json");

        var builder = new FirestoreDbBuilder
        {
            ProjectId = "travel-378415",
            Credential = GoogleCredential.FromFile(credentialPath)
        };

        return builder.Build();
    }
}