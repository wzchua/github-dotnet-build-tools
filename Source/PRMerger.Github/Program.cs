using System;
using System.Threading.Tasks;
using CommandLine;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace PRMerger.Github
{
    class Program
    {
        private class PrMergerOptions
        {
            
            [Option('p', "path", Required = true, HelpText = "path to repo directory")]
            public string Directory { get; set; } = string.Empty;
            [Option('n', "name", Required = true, HelpText = "name of commit author")]
            public string Name { get; set; } = string.Empty;
            [Option('m', "email", Required = true, HelpText = "email of commit author")]
            public string Email { get; set; } = string.Empty;

            [Option('a', "api", Required = true, HelpText = "Github v3 API url")]
            public string GithubApiUrl { get; set; } = string.Empty;
            [Option('o', "owner", Required = true, HelpText = "Github Repository Owner Name")]
            public string GithubRepoOwner { get; set; } = string.Empty;
            [Option('r', "repo-name", Required = true, HelpText = "Github Repository Name")]
            public string GithubRepoName { get; set; } = string.Empty;
            [Option('t', "token", Required = true, HelpText = "Github Repository PR Access API Token")]
            public string GithubSecret { get; set; } = string.Empty;
        }
        static async Task<int> Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<PrMergerOptions>(args);
            PrMergerOptions? option = parseResult.MapResult(
                prMergerOptions => prMergerOptions, 
                _ => (PrMergerOptions?)null);
            
            if (option is null)
            {
                return 1;
            }
            
            var identity = new GithubIdentity
            {
                Name = option.Name,
                Email = option.Email,
                GithubApiUrl = option.GithubApiUrl,
                GithubRepoOwner = option.GithubRepoOwner,
                GithubRepoName = option.GithubRepoName,
                GithubSecret = option.GithubSecret
            };
            var repoRoot = option.Directory;
            var url = new Uri(option.GithubApiUrl);
            var tokenAuth = new Credentials(option.GithubSecret);
            var githubClient = new GitHubClient(new ProductHeaderValue("PRMerger"), url);
            githubClient.Credentials = tokenAuth;
            using (var repo = new Repository(repoRoot))
            {
                        
                var merger = new PrMerger(identity, repo, githubClient);

                var isMergeSuccess = await merger.MergePrIntoTargetAsync();
                return isMergeSuccess ? 0 : 1;
            }
        }
    }
}