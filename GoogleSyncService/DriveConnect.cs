using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace GoogleSyncService
{
    class DriveConnect
    {
        static string ApplicationName = "GoogleDriveSync";
        public static DriveService Service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = GetCardianlities(),
            ApplicationName = ApplicationName,
        });



        public static UserCredential GetCardianlities()  /// passed to helper 
        {
            UserCredential credential;
            string[] Scopes = {DriveService.Scope.Drive, DriveService.Scope.DriveFile,DriveService.Scope.DriveMetadata,DriveService.Scope.DriveAppdata,DriveService.Scope.DriveScripts
            };
            using (var stream = new FileStream(ConfigManager.CardinalitiesPath, FileMode.Open, FileAccess.ReadWrite))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets, Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            return credential;
        }
    }
}
