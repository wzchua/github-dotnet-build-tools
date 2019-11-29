using System;
using LibGit2Sharp;

namespace PRMerger.Github
{
    public static class SigningExtension
    {
        public static Signature SignOff(this GithubIdentity identity)
        {
            return new Signature(identity.Name, identity.Email, DateTimeOffset.Now);
        }
    }
    
}