using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebDAVClient;

namespace CloudDriveShell.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void Test1()
        {
            var tempFolder = System.IO.Path.GetTempPath();
        }

        [TestMethod()]
        public void WebDavTest()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            // Basic authentication required
            IClient c = new Client(new NetworkCredential { UserName = "admin", Password = "123456" });
            c.Server = "http://10.6.0.163/";
            c.BasePath = "/remote.php/webdav/";
            // List items in the root folder
            var files = await c.List();
            // Find first folder in the root folder
            var folder = files.FirstOrDefault(f => f.Href.Contains("test"));

            await c.MoveFolder(
                "/remote.php/webdav/Photo1132133123/124124124111/555123/555566111444/aaa/中文/",
                "/remote.php/webdav/Photo1132133123/124124124111/555123/555566111444/aaa/中文111/");
            // Load a specific folder
            var folderReloaded = await c.GetFolder(folder.Href);

            // List items in the folder
            var folderFiles = await c.List(folderReloaded.Href);
            // Find a file in the folder
            var folderFile = folderFiles.FirstOrDefault(f => f.IsCollection == false);

            var tempFileName = Path.GetTempFileName();

            // Download item into a temporary file
            using (var tempFile = File.OpenWrite(tempFileName))
            using (var stream = await c.Download(folderFile.Href))
                await stream.CopyToAsync(tempFile);

            // Update file back to webdav
            var tempName = Path.GetRandomFileName();
            using (var fileStream = File.OpenRead(tempFileName))
            {
                var fileUploaded = await c.Upload(folder.Href, fileStream, tempName);
            }

            // Create a folder
            var tempFolderName = Path.GetRandomFileName();
            var isfolderCreated = await c.CreateDir("/", tempFolderName);

            var tempFolderName2 = Path.GetRandomFileName();
            var isfolderRename = await c.MoveFolder("/" + tempFolderName, "/" + tempFolderName2);

            // Delete created folder
            var folderCreated = await c.GetFolder("/" + tempFolderName2);
            await c.DeleteFolder(folderCreated.Href);

        }
    }

}
