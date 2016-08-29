using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotfixBranchesInconsistencyDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            // Comment in hotfix-1
            string repoRoot = getRepoRootDir();
            Console.WriteLine(repoRoot);
            Console.ReadLine();
        }
        
        private static string getRepoRootDir()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            return getRepoRootDir(currentDirectory);
        }

        private static string getRepoRootDir(string currentDirectory)
        {
            Console.WriteLine(currentDirectory);
            string gitDirPath = Path.Combine(currentDirectory, ".git");
            if (Directory.Exists(gitDirPath))
            {
                return currentDirectory;
            }

            DirectoryInfo parentDirInfo = Directory.GetParent(currentDirectory);
            if(parentDirInfo == null)
            {
                throw new Exception("No git repository found.");
            }

            return getRepoRootDir(parentDirInfo.FullName);
        }
    }
}
