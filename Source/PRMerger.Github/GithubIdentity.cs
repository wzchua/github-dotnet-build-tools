namespace PRMerger.Github
{
    public class GithubIdentity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string GithubApiUrl { get; set; } = string.Empty;
        public string GithubRepoOwner { get; set; } = string.Empty;
        public string GithubRepoName { get; set; } = string.Empty;
        public string GithubSecret { get; set; } = string.Empty;
    }
}