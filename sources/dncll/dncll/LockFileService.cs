using System.IO;
using NuGet.ProjectModel;

namespace dncll
{
    public class LockFileService
    {
        public LockFile GetLockFile(string projectPath, string outputPath)
        {
            // Run the restore command
            var dotNetRunner = new DotNetRunner();
            var arguments = new[] { "restore", $"\"{projectPath}\"" };
            _ = dotNetRunner.Run(Path.GetDirectoryName(projectPath), arguments);

            // Load the lock file
            var lockFilePath = Path.Combine(outputPath, "project.assets.json");
            return LockFileUtilities.GetLockFile(lockFilePath, NuGet.Common.NullLogger.Instance);
        }
    }
}
