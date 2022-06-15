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

        static void Main(string[] args)

        {
            string pathDemoApp = ConfigurationManager.AppSettings["pathDemoApp"];
            MonitorDirectory(pathDemoApp);
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
                                         new UpdateFileRequest("My updated file", "SucceededSucceededSucceeded", fileDetails.First().Sha));
            }
            catch (Octokit.NotFoundException)
            {

                await ghClient.Repository.Content.CreateFile(owner, repo, path, new CreateFileRequest("API File cs creation", "Hello Universe! " + DateTime.UtcNow, master));
            }
        }
        private static void ReadXmlFile(string path)
        {
            string xmlFilePath = ConfigurationManager.AppSettings["xmlFilePath"];

           // string xmlFilePath = @"C:\Users\User\Desktop\Project\WatcherProject\WatcherProject\WatcherProject1\bin\Debug\TestResult.xml";
            Serializer ser = new Serializer();
            string xmlInputData = string.Empty;
            string xmlOutputData = string.Empty;

            xmlInputData = File.ReadAllText(xmlFilePath);

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

            string solutionFile = ConfigurationManager.AppSettings["solutionFile"];
            string MSBuild = ConfigurationManager.AppSettings["MSBuild"];


            var pro = Process.Start(MSBuild, solutionFile);
            pro.WaitForExit();

            string nunitConsole = ConfigurationManager.AppSettings["nunitConsole"];
            string nunitDLL = ConfigurationManager.AppSettings["nunitDLL"];

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
