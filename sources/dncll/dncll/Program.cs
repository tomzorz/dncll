using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NuGet.ProjectModel;

namespace dncll
{
    internal class Program
    {
        private static readonly List<string> Dependencies = new List<string>();

        // TODO add formatting choice output

        static async Task Main(string[] args)
        {
#if !DEBUG
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments! Please provide path to an .sln, then an output txt file.");
                return;
            }
#endif

            // Replace to point to your project or solution
#if DEBUG
            var projectPath = args.Any() && args.Length == 1 ? args[0] : @"D:\Projects_development\OpenSource\dncll\sources\dncll\dncll.sln";
            var outputPath = $"out.{DateTime.Now:yy-MM-dd-hh-mm}.txt";
#else
            var projectPath = args[0];
            var outputPath = args[1];
#endif

            var dependencyGraphService = new DependencyGraphService();
            var dependencyGraph = dependencyGraphService.GenerateDependencyGraph(projectPath);

            foreach (var project in dependencyGraph.Projects.Where(p => p.RestoreMetadata.ProjectStyle == ProjectStyle.PackageReference))
            {
                // Generate lock file
                var lockFileService = new LockFileService();
                var lockFile = lockFileService.GetLockFile(project.FilePath, project.RestoreMetadata.OutputPath);

                //Console.WriteLine(project.Name);

                foreach (var targetFramework in project.TargetFrameworks)
                {
                    //Console.WriteLine($"  [{targetFramework.FrameworkName}]");

                    var lockFileTargetFramework = lockFile.Targets.FirstOrDefault(t => t.TargetFramework.Equals(targetFramework.FrameworkName));
                    if (lockFileTargetFramework != null)
                    {
                        foreach (var dependency in targetFramework.Dependencies)
                        {
                            var projectLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == dependency.Name);

                            ReportDependency(projectLibrary, lockFileTargetFramework, 1);
                        }
                    }
                }
            }

            // dicts
            var licenseToLib = new Dictionary<string, string>();

            // dedupe + sort
            var dep2 = Dependencies.Distinct().OrderBy(x => x).ToList();

            // get nuspec + license urls

            var hc = new HttpClient();

            foreach (var s in dep2)
            {
                // https://api.nuget.org/v3-flatcontainer/newtonsoft.json/6.0.4/newtonsoft.json.nuspec
                Console.WriteLine($"Getting NuSpec for {s}...");

                var split = s.Split(',');
                var name = split[0].ToLowerInvariant();
                var version = split[1].ToLowerInvariant();

                var nuspec = await hc.GetStringAsync($"https://api.nuget.org/v3-flatcontainer/{name}/{version}/{name}.nuspec");
                const string luBegin = "<licenseUrl>";
                const string luEnd = "</licenseUrl>";

                var start = nuspec.IndexOf(luBegin, StringComparison.Ordinal);
                if (start == -1)
                {
                    // TODO handle no license in it
                }
                else
                {
                    start += luBegin.Length;
                    var end = nuspec.IndexOf(luEnd, StringComparison.Ordinal);

                    var licenseUrl = nuspec.Substring(start, end - start);

                    licenseToLib[s] = licenseUrl;
                }
            }

            var sb = new StringBuilder();
            foreach (var ltl in licenseToLib)
            {
                var split = ltl.Key.Split(',');
                var name = split[0].ToLowerInvariant();
                var version = split[1].ToLowerInvariant();

                sb.AppendLine($"{name}, v{version} - <a href=\"{ltl.Value}\">{ltl.Value}</a>");
            }
            File.WriteAllText(outputPath, sb.ToString());

            Console.WriteLine("done...");
            Console.ReadLine();
        }

        private static void ReportDependency(LockFileTargetLibrary projectLibrary, LockFileTarget lockFileTargetFramework, int indentLevel)
        {
            //Console.Write(new String(' ', indentLevel * 2));
            //Console.WriteLine($"{projectLibrary.Name}, v{projectLibrary.Version}");

            Dependencies.Add($"{projectLibrary.Name},{projectLibrary.Version}");

            foreach (var childDependency in projectLibrary.Dependencies)
            {
                var childLibrary = lockFileTargetFramework.Libraries.FirstOrDefault(library => library.Name == childDependency.Id);

                ReportDependency(childLibrary, lockFileTargetFramework, indentLevel + 1);
            }
        }
    }
}
