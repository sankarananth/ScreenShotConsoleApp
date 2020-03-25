using ScreenTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.Vision.v1;

namespace ScreenShotConsoleApp
{
	class Program
	{
        static DirectoryInfo di;
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Capture Demo";
        static void Main(string[] args)
        {

            GoogleCredential credential;

            credential = GetCredentials();

            di = new DirectoryInfo("C:\\ss");
                if (!di.Exists) { di.Create(); }

                PrintScreen ps = new PrintScreen();
                ps.CaptureScreenToFile(di + "\\screen.png", System.Drawing.Imaging.ImageFormat.Png);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            UploadBasicImage(di+"\\screen.png", service);


        }
        private static void UploadBasicImage(string path, DriveService service)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Name = Path.GetFileName(path);
            fileMetadata.MimeType = "image/jpeg";
            Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "image/jpeg");
                request.Fields = "id";
                request.Upload();
                 var file = request.ResponseBody;

            Console.WriteLine("File ID: " + file.Id);
            }

           

        }
        private static GoogleCredential GetCredentials()
        {
            GoogleCredential credential;

            using (var stream = new FileStream("My Project-8157d0250f56.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleCredential.FromStream(stream)
        .CreateScoped(VisionService.Scope.CloudPlatform);
                // Console.WriteLine("Credential file saved to: " + credPath);
            }

            return credential;
        }
    }
}
