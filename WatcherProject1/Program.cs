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
        static string owner = "mosmo46";
        static string name = "gitignore";

        //static InMemoryCredentialStore credentials = new InMemoryCredentialStore(new Credentials("your-token-here"));
      //  static ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("ophion"), credentials);

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


        private static void ComparisonBetween()
        {

            var ghe = new Uri("https://github.com/mosmo46/DemoApp");

            var client1 = new GitHubClient(new ProductHeaderValue("DemoApp"), ghe);


            Console.WriteLine($"client1=>>{client1}");



        }
        private static async void uplodaToGithub(string path)
        {
            var ghClient = new GitHubClient(new ProductHeaderValue("DemoApp"));

            ghClient.Credentials = new Credentials("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");

           // var owner = "mosmo46";
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


     
        private static async Task CanGetFilesInCommit()
        {




            //var repo = "DemoApp";

            //var client = new GitHubClient(new ProductHeaderValue("DemoApp"));

            //var repository = await client.Repository.Commit.GetAll(owner, repo);

            //var commitsFiltered = repository.Select(async (_) =>
            //{
            //    return await client.Repository.Commit.Compare(owner, repo, _.Sha);
            //}).ToList();

            //var commits = await Task.WhenAll(commitsFiltered);

            //foreach (var item in commits)
            //{
            //    Console.WriteLine($"commitsitem: => {item.Commit.NodeId}");

            //}

            //var repo = "DemoApp";
            //var ghClient = new GitHubClient(new ProductHeaderValue("DemoApp"));
            //string sha1 = await ghClient.Repository.Commit.GetSha1(owner, repo, repo);

                //var commit = await ghClient.Repository.Commit.Get(owner, repo, "c8108fad4ec7abac9f73bea2b4aa98cb2c9be343");
                //Console.WriteLine($"commit.Filescommit.Files===>>>{commit.Files}");
            }
        //private static async Task Main2E()
        //{

        //    var repo1 = "DemoApp";

        //    //Get branch info
        //    GitHubClient client = new GitHubClient(new ProductHeaderValue("DemoApp"));
        //    Repository repo = await client.Repository.Get(owner, repo1);
        //    Branch branch = await client.Repository.Branch.Get(repo.Id, repo.DefaultBranch);
        //    string sha1 = await client.Repository.Commit.GetSha1(repo.Id, branch.Commit.Sha);

        //    var commit = await client.Repository.Commit.Get(owner, repo1, sha1);

        //    Console.WriteLine($"commit.Filescommit.Files===>>>{commit.Files}");
        //    //Print info
        //    Console.WriteLine("Repository: " + repo.FullName);
        //    Console.WriteLine("Branch: " + branch.Name);
        //    Console.WriteLine("Branch SHA: " + branch.Commit.Sha);
        //    Console.WriteLine("Branch SHA1: " + sha1);

        //    //Keep info on screen
        //    Console.WriteLine("\nPress enter to quit...");
        //    Console.ReadLine();
        //}
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
            //_ = CanGetFilesInCommit();

            // _ = Main2E();
            ComparisonBetween();
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
