using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatcherProject1
{
    class Program

    {

        static void Main(string[] args)

        {

        string path = @"C:\Users\User\Desktop\Project\DemoApp\DemoApp";

            MonitorDirectory(path);
            Console.ReadKey();

        }

      

        private static void MonitorDirectory(string path)

        {

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = path;
            fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Filter = "*.cs";
            fileSystemWatcher.IncludeSubdirectories = true;


            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
            
        }
        private static async void uplodaToGithub(string path)
        {
            var ghClient = new GitHubClient(new ProductHeaderValue("DemoApp"));

            ghClient.Credentials = new Credentials("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");

            var owner = "mosmo46";
            var repo = "DemoApp";
            var master = "master";
            try
            {
                var fileDetails = await ghClient.Repository.Content.GetAllContentsByRef(owner, repo,
                                        path, master);
                var updateResult = await ghClient.Repository.Content.UpdateFile(owner, repo, path,
                                         new UpdateFileRequest("My updated file", "Succeeded", fileDetails.First().Sha));
            }
            catch (Octokit.NotFoundException)
            {

                await ghClient.Repository.Content.CreateFile(owner, repo, path, new CreateFileRequest("API File cs creation", "Hello Universe! " + DateTime.UtcNow, master));
            }
        }
        private static void ReadXmlFile(string path)
        {
            string filename = @"C:\Users\User\Desktop\Project\WatcherProject\WatcherProject\WatcherProject1\bin\Debug\TestResult.xml";
            Serializer ser = new Serializer();
            string xmlInputData = string.Empty;
            string xmlOutputData = string.Empty;

            xmlInputData = File.ReadAllText(filename);

            XmlModel.testrun resFromXml = ser.Deserialize<XmlModel.testrun>(xmlInputData);
            xmlOutputData = ser.Serialize<XmlModel.testrun>(resFromXml);

            Console.WriteLine(xmlOutputData);
            if (resFromXml.failed == 0)
            {
                uplodaToGithub(path);
            }
            else
            {
                Console.WriteLine("One or more of the tests do not pass");
            }
        }
        private static  void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string solutionFile = @"C:\Users\User\Desktop\Project\WatcherProject\WatcherProject\WatcherProject.sln";
            string MSBuild = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe";
            var pro = Process.Start(MSBuild, solutionFile);
            pro.WaitForExit();
            string nunitConsole = @"C:\Users\User\.nuget\packages\nunit.consolerunner\3.15.0\tools\nunit3-console.exe";
            
            string nunitDLL = @"C:\Users\User\Desktop\Project\DemoApp\DemoApp\UnitTestProject1\bin\Debug\UnitTestProject1.dll";

            var processRnnar = Process.Start(nunitConsole, nunitDLL);

            processRnnar.WaitForExit();

            ReadXmlFile(e.Name);

            Console.ReadLine();
            Console.WriteLine("File created: {0}", e.Name);
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)

        {
            Console.WriteLine("File created: {0}", e.Name);

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
