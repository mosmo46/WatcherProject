using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WatcherProject1
{
    class Program
    {
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        static void Main(string[] args)

        {
            WatchFile(watcher);
            Console.ReadLine();

            //  var pathDemoApp = ConfigurationManager.AppSettings["pathDemoApp"].Split(',');


            //for (int i = 0; i < pathDemoApp.Length; i++)
            //{
            //    if (Directory.Exists(pathDemoApp[i]))
            //    {
            //        MonitorDirectory(pathDemoApp[i]);
            //        break;
            //    }
            //}


        }
        // set up the FileSystemWatcher object
        private static void WatchFile(FileSystemWatcher watcher)
        {
            var pathDemoApp = @"C:\Users\User\Desktop\Project\DemoApp\DemoApp";
            // ***** Change this as required
            watcher.Path = pathDemoApp;

            // For this example, I only care about when new files are
            // created
            watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

            // Add event handlers: 1 for the event raised when a file
            // is created, and 1 for when it detects an error.
            watcher.Changed += new FileSystemEventHandler(FileSystemWatcher_Changed);
            watcher.Created += new FileSystemEventHandler(FileSystemWatcher_Created);
            watcher.Deleted += new FileSystemEventHandler(FileSystemWatcher_Deleted);
        //    watcher.Renamed += new FileSystemEventHandler(FileSystemWatcher_Renamed);
           
            
            
            watcher.Error += new ErrorEventHandler(WatcherError);
           //    watcher.Filter = "*.cs";

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }
        // Define the found event handler.
        private static void NewFile(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("A file has been found!");
           // File.Delete(e.FullPath);
        }

        // The error event handler
        private static void WatcherError(object source, ErrorEventArgs e)
        {
            Exception watchException = e.GetException();
            Console.WriteLine($"A FileSystemWatcher error has occurred: {watchException.Message}");
            // We need to create new version of the object because the
            // old one is now corrupted
            watcher = new FileSystemWatcher();
            while (!watcher.EnableRaisingEvents)
            {
                try
                {
                    // This will throw an error at the
                    // watcher.NotifyFilter line if it can't get the path.
                    WatchFile(watcher);
                    Console.WriteLine("I'm Back!!");
                }
                catch
                {
                    // Sleep for a bit; otherwise, it takes a bit of
                    // processor time
                    System.Threading.Thread.Sleep(5000);
                }
            }

        }

            //private static void MonitorDirectory(string path)

            //{

            //    FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(path);

            //    fileSystemWatcher.Path = path;

            //    fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
            //                        | NotifyFilters.CreationTime
            //                        | NotifyFilters.DirectoryName
            //                        | NotifyFilters.FileName
            //                        | NotifyFilters.LastAccess
            //                        | NotifyFilters.LastWrite
            //                        | NotifyFilters.Security
            //                        | NotifyFilters.Size;

            //    fileSystemWatcher.Created += FileSystemWatcher_Created;
            //    fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            //    fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            //    fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            //    fileSystemWatcher.EnableRaisingEvents = true;
            //    fileSystemWatcher.Filter = "*.cs";
            //    fileSystemWatcher.IncludeSubdirectories = true;


            //    Console.WriteLine("Press enter to exit.");
            //    Console.ReadLine();

            //}

            private static async void UplodaToGithub(string path)
        {
            var ghClient = new GitHubClient(new ProductHeaderValue("DemoApp"));

            ghClient.Credentials = new Credentials("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");
            string owner = "mosmo46";
            var repo = "DemoApp";
            var master = "master";
            try
            {
                var fileDetails = await ghClient.Repository.Content.GetAllContentsByRef(owner, repo,
                                        path, master);
                var updateResult = await ghClient.Repository.Content.UpdateFile(owner, repo, path,
                                         new UpdateFileRequest("My updated file", "Succeed", fileDetails.First().Sha));
            }
            catch (Octokit.NotFoundException)
            {
                await ghClient.Repository.Content.CreateFile(owner, repo, path, new CreateFileRequest("API File cs creation", "Hello Universe! " + DateTime.UtcNow, master));
            }
        }

        private static void ReadXmlFile(string path)
        {
            Serializer ser = new Serializer();
            string xmlInputData = string.Empty;
            string xmlOutputData = string.Empty;
            var xmlFilePaths = ConfigurationManager.AppSettings["xmlFilePath"].Split(',');
            foreach (var xmlFilePath in xmlFilePaths)
            {

                if (File.Exists(xmlFilePath))
                {   
                    xmlInputData = File.ReadAllText(xmlFilePath);

                    XmlModel.testrun resFromXml = ser.Deserialize<XmlModel.testrun>(xmlInputData);

                    xmlOutputData = ser.Serialize<XmlModel.testrun>(resFromXml);

                    Console.WriteLine(xmlOutputData);
                    if (resFromXml.failed == 0)
                    {
                        UplodaToGithub(path);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("One or more of the tests do not pass");
                    }
                }
            }


        }


        private static void MsBuild()
        {
            string[] solutionFiles = ConfigurationManager.AppSettings["solutionFile"].Split(',');
            string[] MSBuilds = ConfigurationManager.AppSettings["MSBuild"].Split(',');
            var solutionFile = string.Empty;
            var MSBuild = string.Empty;


            foreach (var tmpSolutionFile in solutionFiles)
            {
                if (File.Exists(tmpSolutionFile))
                {
                    solutionFile = tmpSolutionFile;
                break;
                }
            }

            foreach (var tmpMSBuild in MSBuilds)
            {
                if (File.Exists(tmpMSBuild))
                {
                    MSBuild = tmpMSBuild;
                    break;
                }
            }

            var processBuild = Process.Start(MSBuild, solutionFile);
            processBuild.WaitForExit();
        }
        private static void RunTests()
        {
            string[] nunitConsoles = ConfigurationManager.AppSettings["nunitConsole"].Split(',');
            string[] nunitDLLs = ConfigurationManager.AppSettings["nunitDLL"].Split(',');
            string nunitConsole = string.Empty;
            string nunitDLL = string.Empty;

            foreach (var tmpNunitConsole in nunitConsoles)
            {
                if (File.Exists(tmpNunitConsole))
                {
                    nunitConsole = tmpNunitConsole;
                    break;
                }
            }

            foreach (var tmpnunitDLLs in nunitDLLs)
            {
                if (File.Exists(tmpnunitDLLs))
                {
                    nunitDLL = tmpnunitDLLs;
                    break;
                }
            }
            var processRnnar = Process.Start(nunitConsole, nunitDLL);

            processRnnar.WaitForExit();
        }

        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {

            MsBuild();

            RunTests();

            ReadXmlFile(e.FullPath);
            Console.WriteLine("File Changed: {0}", e.Name);
            Console.WriteLine($"DateTime.Now=>{DateTime.Now}");
           
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)

        {
            Console.WriteLine("File created: {0}", e.FullPath);

        }

        private static void FileSystemWatcher_Renamed(object sender, FileSystemEventArgs e)

        {

            Console.WriteLine("File renamed: {0}", e.Name);

        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)

        {
            Console.WriteLine("File deleted: {0}", e.Name);
        }
    }
}
