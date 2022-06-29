using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatcherProject1
{
    class Program
    {
        private static int counter=0;

        static void Main(string[] args)

        {

        
            ChecksForChangesInReopAndpull();

            var pathDemoApp = ConfigurationManager.AppSettings["pathDemoApp"].Split(',');

            for (int i = 0; i < pathDemoApp.Length; i++)
            {
                if (Directory.Exists(pathDemoApp[i]))
                {
                    MonitorDirectory(pathDemoApp[i]);
                    break;
                }
            }



        }


        private static void ChecksForChangesInReopAndpull()

        {
            while (true)
            {

                using (var repo = new LibGit2Sharp.Repository(@"C:\Users\User\source\repos\DemoApp"))
                {

                          string gitUser = "Moshe Yaso";
                           string gitToken = "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";
        var trackingBranch = repo.Branches["remotes/origin/my-remote-branch"];

                    
                    Tag t = repo.ApplyTag($"{counter + 1}");
                    foreach (var tag in repo.Tags)
                    {
                        PushOptions options = new PushOptions();
                        options.CredentialsProvider = new CredentialsHandler(
                            (url, usernameFromUrl, types) =>
                                new UsernamePasswordCredentials()
                                {
                                    Username = gitUser,
                                    Password = gitToken

                                });
                        repo.Network.Push(repo.Network.Remotes["origin"], tag.CanonicalName, options);
                        counter++;
                    }
                    PullOptions pullOptions = new PullOptions()
                    {
                        MergeOptions = new MergeOptions()
                        {
                            FastForwardStrategy = FastForwardStrategy.Default
                        }
                    };


                    MergeResult mergeResult = Commands.Pull(
                        repo,
                        new LibGit2Sharp.Signature("my name", "my email", DateTimeOffset.Now),
                        pullOptions
                    );


                    if (mergeResult.Commit != null)
                    {
                        Console.WriteLine("pull changes successfully");
                        MsBuild();
                        RunTests();
                    }
                    else
                    {
                        Console.WriteLine("no changes to pull");
                    }
                }
                Thread.Sleep(10000);

            }
        }



        private static void MonitorDirectory(string path)

        {

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(path);

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
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();

        }
        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
           MsBuild();
           RunTests();

          //  ReadXmlFile(e.Name, e.FullPath);

            Console.WriteLine("File Changed: {0}", e.Name);
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



        //private static void ReadXmlFile(string path, string readFilePath)
        //{
        //    Serializer ser = new Serializer();
        //    string xmlInputData = string.Empty;
        //    string xmlOutputData = string.Empty;
        //    var xmlFilePaths = ConfigurationManager.AppSettings["xmlFilePath"].Split(',');


        //    foreach (var xmlFilePath in xmlFilePaths)
        //    {

        //        if (File.Exists(xmlFilePath))
        //        {
        //            xmlInputData = File.ReadAllText(xmlFilePath);

        //            XmlModel.testrun resFromXml = ser.Deserialize<XmlModel.testrun>(xmlInputData);

        //       //     xmlOutputData = ser.Serialize<XmlModel.testrun>(resFromXml);

        //            if (resFromXml.failed == 0)
        //            {
        //                UplodaToGithub(path, readFilePath);
        //                break;
        //            }
        //            else
        //            {
        //                Console.WriteLine("One or more of the tests do not pass");
        //            }
        //        }
        //    }


        //}
        //private static async void UplodaToGithub(string path, string readFilePath)
        //{
        //    var ghClient = new GitHubClient(new ProductHeaderValue("DemoApp"));

        //    ghClient.Credentials = new Octokit.Credentials("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");
        //    string owner = "mosmo46";
        //    var repo = "DemoApp";
        //    var master = "master";
        //    try
        //    {
        //        var fileDetails = await ghClient.Repository.Content.GetAllContentsByRef(owner, repo,
        //                                path, master);
        //        var sha = fileDetails.First().Sha;

        //        var updateResult = await ghClient.Repository.Content.UpdateFile(owner, repo, path,
        //                                 new UpdateFileRequest("My updated file", ReadCSFile(readFilePath), sha));

        //    }
        //    catch (Octokit.NotFoundException)
        //    {
        //        await ghClient.Repository.Content.CreateFile(owner, repo, path, new CreateFileRequest("API File cs creation", "Hello Universe! " + DateTime.UtcNow, master));
        //    }
        //}
        //private static string ReadCSFile(string readFilePath)
        //{
        //        string content = System.IO.File.ReadAllText(readFilePath);
        //    return content;
        //}





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
