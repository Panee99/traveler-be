using System.Reflection;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Shared.ExternalServices.Firebase;

public static class FirebaseAppHelper
{
    public static void Init()
    {
        var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var credentialPath = Path.Combine(projectPath, "ExternalServices", "Firebase", "firebase-admin-sdk.json");

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialPath)
        });
    }

    public static FirebaseMessaging GetMessaging()
    {
        if (FirebaseMessaging.DefaultInstance is null)
            throw new Exception("Firebase Messaging not initialized.");

        return FirebaseMessaging.DefaultInstance;
    }
}