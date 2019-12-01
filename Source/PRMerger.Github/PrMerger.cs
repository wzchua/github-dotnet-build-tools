using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibGit2Sharp;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace PRMerger.Github
{
    public class PrMerger
    {
        private static readonly string PrRegexStr = "^pull\\/(?<prNumber>\\d+)$";
        private static readonly Regex PrRegex = new Regex(PrRegexStr, RegexOptions.Compiled);
        
        private readonly GithubIdentity _gitIdentity;
        private readonly IRepository _repository;
        private readonly IGitHubClient _gitHubClient;
        
        public PrMerger(GithubIdentity gitIdentity, IRepository repository, IGitHubClient gitHubClient)
        {
            _gitIdentity = gitIdentity;
            _repository = repository;
            _gitHubClient = gitHubClient;
        }

        public async Task<bool> MergePrIntoTargetAsync()
        {
            var currentBranch = _repository.Head;
            var currentBranchName = currentBranch.FriendlyName;
            var prNumber = ParsePrNumber(currentBranchName);
            if(prNumber is null)
            {
                return false;
            }
            var prTargetBranchName = await FindPrMergeTargetAsync((int)prNumber);

            Commands.Checkout(_repository, _repository.Branches[prTargetBranchName]);
            var prBaseBranch = _repository.CreateBranch($"pr/{prTargetBranchName}/{prNumber}/merged");
            Commands.Checkout(_repository, prBaseBranch);
                
            var result = _repository.Merge(currentBranch, _gitIdentity.SignOff());
            if (result.Status == MergeStatus.Conflicts)
            {
                Console.Error.WriteLine("Merge conflict");
                _repository.Reset(ResetMode.Hard);
                return false;
            }
            Console.WriteLine("Merge succeeded");
            return true;
        }
        private int? ParsePrNumber(string prBranchName)
        {
            var match = PrRegex.Match(prBranchName);
            if (match.Success)
            {
                return int.Parse(match.Groups["prNumber"].Value);
            }
            Console.Error.WriteLine("Invalid branch for a pull request");
            return null;
        }

        private async Task<string> FindPrMergeTargetAsync(int prNumber)
        {
            var pr = await _gitHubClient.PullRequest.Get(
                _gitIdentity.GithubRepoOwner,
                _gitIdentity.GithubRepoName,
                prNumber);

            return pr.Base.Ref;
        }
    }
}