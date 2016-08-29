using LibGit2Sharp;
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
            string repoRoot = getRepoRootDir();
            using (var repo = new Repository(repoRoot))
            {
                Console.WriteLine(repo.Head.Name);
                checkAllPreviousHotfixBranches(repo);
            }

            Console.ReadLine();
        }

        private static void checkAllPreviousHotfixBranches(Repository repo)
        {
            repo.Network.Fetch(repo.Network.Remotes["origin"]);
            foreach(Branch branch in repo.Branches.Where(b => b.IsRemote && !b.Name.Contains(repo.Head.Name)))
            {
                Console.WriteLine("Checking branch " + branch.CanonicalName);
                HashSet<string> missingCommits = new HashSet<string>();
                if(checkCurrentBranchAgainstPrevious(repo, branch, out missingCommits))
                {
                    Console.WriteLine("Branch OK");
                }
                else
                {
                    Console.WriteLine("Branch contains commits missing from current branch");
                    foreach(string missing in missingCommits)
                    {
                        Console.WriteLine(missing);
                    }
                }
            }
        }

        // returns false if there are commits in the previous branch that are missing from the current
        private static bool checkCurrentBranchAgainstPrevious(Repository repo, Branch prevHotfix, out HashSet<string> prevHotfixCommits)
        {
            // git log HEAD..prevHotfix - all commits in prevHotfix not in HEAD
            // git log prevHotfix..HEAD - all commits in HEAD not in prevHotfix
            prevHotfixCommits = new HashSet<string>();

            var filter = new CommitFilter { Since = repo.Head, Until = prevHotfix };
            foreach(Commit commit in repo.Commits.QueryBy(filter))
            {
                prevHotfixCommits.Add(commit.Message);
            }

            filter = new CommitFilter { Since = prevHotfix, Until = repo.Head };
            foreach(Commit commit in repo.Commits.QueryBy(filter))
            {
                if(prevHotfixCommits.Contains(commit.Message))
                {
                    prevHotfixCommits.Remove(commit.Message);
                }
            }

            if(prevHotfixCommits.Count > 0)
            {
                return false;
            }

            return true;
        }

        private static string getRepoRootDir()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            return getRepoRootDir(currentDirectory);
        }

        private static string getRepoRootDir(string currentDirectory)
        {
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
