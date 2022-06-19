﻿using Octokit;
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


        static void Main(string[] args)

        {
            var pathDemoApp = ConfigurationManager.AppSettings["pathDemoApp"].Split(',');
            for (int i = 0; i < pathDemoApp.Length; i++)
            {
                if (Directory.Exists(pathDemoApp[i]))
                {
                    MonitorDirectory(pathDemoApp[i]);
                    break;
                }
            }

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
                        uplodaToGithub(path);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("One or more of the tests do not pass");
                    }
                }
            }


        }



        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
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

            var pro = Process.Start(MSBuild, solutionFile);
            pro.WaitForExit();

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
