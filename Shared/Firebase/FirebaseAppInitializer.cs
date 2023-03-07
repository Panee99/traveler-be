using System.Reflection;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace Shared.Firebase;

public static class FirebaseAppInitializer
{
    public static void Init()
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var credentialPath = Path.Combine(projectPath, "Firebase", "firebase-admin-sdk.json");

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credentialPath)
        });
    }
}