using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ScreenTest;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;

namespace DriveQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.Drive};
        static string ApplicationName = "Drive API .NET Quickstart";
        UserCredential credential;
        public  UserCredential GetCredential()
        {
            using (var stream =
               new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            return credential;

        }
        static void Main(string[] args)
        {
            Program p = new Program();
            
            string folderCreation = CreateFolder("Screenshot");
            string path = AppDomain.CurrentDomain.BaseDirectory;
            PrintScreen ps = new PrintScreen();
            ps.CaptureScreenToFile(path + "\\screen.jpg", ImageFormat.Jpeg);
            if (folderCreation!= "Sorry but the file Screenshot already exists!")
            {
                
                
                var fileMetadata = new File()
                {
                    Name = "screenshot.jpg",
                    Parents = new List<string> { folderCreation }
                };
                FilesResource.CreateMediaUpload request;
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = p.GetCredential(),
                    ApplicationName = ApplicationName,
                });

                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";
                using (var stream = new System.IO.FileStream(path + "\\screen.jpg",
                                       System.IO.FileMode.Open))
                {
                    //fileMetadata.Parents.Add()
                    request = service.Files.Create(
                        fileMetadata, stream, "image/jpeg");
                    request.Fields = "id";
                    request.Upload();
                }


                // List files.
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                    .Files;
                Console.WriteLine("Files:");
                if (files != null && files.Count > 0)
                {
                    foreach (var file1 in files)
                    {
                        Console.WriteLine("{0} ({1})", file1.Name, file1.Id);
                    }
                }
                else
                {
                    Console.WriteLine("No files found.");
                }
                Console.Read();

            }
            else
            {
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = p.GetCredential(),
                    ApplicationName = ApplicationName,
                });
                var listRequest = service.Files.List();
                listRequest.PageSize = 100;
                listRequest.Q = "trashed = false and name contains 'Screenshot' and 'root' in parents";
                listRequest.Fields = "files";
                var files = listRequest.Execute().Files;
                string id = "";
                foreach (var file in files)
                {
                    if ("Screenshot" == file.Name)
                        id=file.Id;

                }
                var fileMetadata = new File()
                {
                    Name = "screenshot.jpg",
                    Parents = new List<string> { id }
                };
                
                Console.WriteLine("Folder Already Exists");
                FilesResource.CreateMediaUpload request;
                using (var stream = new System.IO.FileStream(path + "\\screen.jpg",
                                       System.IO.FileMode.Open))
                {
                    //fileMetadata.Parents.Add()
                    request = service.Files.Create(
                        fileMetadata, stream, "image/jpeg");
                    request.Fields = "id";
                    request.Upload();
                }
            }
            
           
            
        }
        public static string CreateFolder(string folderName)
        {
            Program p = new Program();
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = p.GetCredential(),
                ApplicationName = ApplicationName,
            });
            bool exists = Exists(folderName);
            if (exists)
                return $"Sorry but the file {folderName} already exists!";

            var file = new Google.Apis.Drive.v3.Data.File();
            file.Name = folderName;
            file.MimeType = "application/vnd.google-apps.folder";
            var request = service.Files.Create(file);
            request.Fields = "id";
            return request.Execute().Id;
        }
        private static bool Exists(string name)
        {
            Program p = new Program();
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = p.GetCredential(),
                ApplicationName = ApplicationName,
            });
            var listRequest = service.Files.List();
            listRequest.PageSize = 100;
            listRequest.Q = $"trashed = false and name contains '{name}' and 'root' in parents";
            listRequest.Fields = "files(name)";
            var files = listRequest.Execute().Files;
            string id = "";
            foreach (var file in files)
            {
                if (name == file.Name)
                    return true;
                
            }
            return false;
        }
    }
}