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

namespace ScreenShotConsoleApp
{
	class Program
	{
        static DirectoryInfo di;
        static void Main(string[] args)
        {

            

                di = new DirectoryInfo("C:\\ss");
                if (!di.Exists) { di.Create(); }

                PrintScreen ps = new PrintScreen();
                ps.CaptureScreenToFile(di + "\\screen.png", System.Drawing.Imaging.ImageFormat.Png);
                Authorize();
                

        }
        private static void Authorize()
        {
            string[] scopes = new string[] { DriveService.Scope.Drive,
                               DriveService.Scope.DriveFile,};
            var clientId = "12345678-kiwwjelkrklsjdkljklaflkjsdjasdkhw.apps.googleusercontent.com";      // From https://console.developers.google.com  
            var clientSecret = "ksdklfklas2lskj_asdklfjaskla-";          // From https://console.developers.google.com  
                                                                         // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }, scopes,
            Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;
            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   
            string[] file = Directory.GetFiles(@"C:\ss\screen.png");
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyAppName",

            });
            service.HttpClient.Timeout = TimeSpan.FromMinutes(100);
            string fileName = file[0].ToString().Trim();
            //Long Operations like file uploads might timeout. 100 is just precautionary value, can be set to any reasonable value depending on what you use your service for  
            var respocne = uploadFile(service, fileName, "");
            // team drive root https://drive.google.com/drive/folders/0AAE83zjNwK-GUk9PVA   

        }
        public static Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, string _uploadFile, string _parent, string _descrp = "Uploaded with .NET!")
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = _descrp;
                body.MimeType = GetMimeType(_uploadFile);
                // body.Parents = new List<string> { _parent };// UN comment if you want to upload to a folder(ID of parent folder need to be send as paramter in above method)
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.SupportsTeamDrives = true;
                    // You can bind event handler with progress changed event and response recieved(completed event)

                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            System.Diagnostics.Debug.WriteLine(mimeType);
            return mimeType;
        }
    }
}
