using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reddit;
using Reddit.AuthTokenRetriever;
using Reddit.Controllers;
using Reddit.Inputs;
using Reddit.Inputs.Search;
using RedditAlerts.Models;

namespace RedditAlerts.Managers
{
    public class RedditManager
    {
        private readonly RedditClient _redditClient;
        public RedditManager(string appID, string appSecret)
        {
            _redditClient = AuthorizeUser(appID, appSecret); 
        }

        public RedditManager(string appID, string appSecret, string refreshToken)
        {
            _redditClient = new(appID, refreshToken, appSecret);
        }

        public RedditClient AuthorizeUser(string appId, string appSecret, int port = 8080)
        {
            AuthTokenRetrieverLib authTokenRetrieverLib = new (appId, port, "localhost",
                appSecret: appSecret);

            // Start the callback listener.  --Kris
            // Note - Ignore the logging exception message if you see it.  You can use Console.Clear() after this call to get rid of it if you're running a console app.
            authTokenRetrieverLib.AwaitCallback();

            OpenBrowser(authTokenRetrieverLib.AuthURL());
            while(string.IsNullOrWhiteSpace(authTokenRetrieverLib.RefreshToken))
            {

            }
            authTokenRetrieverLib.StopListening();
            return new(appId, authTokenRetrieverLib.RefreshToken, appSecret,
                authTokenRetrieverLib.AccessToken);
        }

        private static void OpenBrowser(string authUrl, string browserPath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")
        {
            try
            {
               
                Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
                //ProcessStartInfo processStartInfo = new ProcessStartInfo(authUrl);
                //Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // This typically occurs if the runtime doesn't know where your browser is.  Use BrowserPath for when this happens.  --Kris
                ProcessStartInfo processStartInfo = new ProcessStartInfo(browserPath)
                {
                    Arguments = authUrl
                };
                Process.Start(processStartInfo);
            }
        }

        public List<Post> SearchPosts(string searchString, string subReddit = "all")
        {
            List<Post> posts;
            if (string.IsNullOrWhiteSpace(subReddit))
            {
                subReddit = "all";
            }
            posts = _redditClient.Subreddit(subReddit)
                    .Search(new SearchGetSearchInput(searchString));
            foreach (Post post in posts)
            {
                DigestedRedditPost dig = new(post);
                dig.Title = post.Title;
            }
            return posts;
        }

        public List<DigestedRedditPost> GetLastHoursPosts(string subReddit,
            string? after = "")
        {
            List<DigestedRedditPost> digestedRedditPosts = new();
            List<Post> posts = _redditClient.Subreddit
                (subReddit).Posts.GetNew(new 
                CategorizedSrListingInput(after: after, limit: 100));
            int postsAfterHour = 0; // sometimes Reddit time travels so we give them 3 strikes
            foreach(Post post in posts)
            {
                if(post.Created < DateTime.Now.AddHours(-1))
                {
                    postsAfterHour += 1;
                    if(postsAfterHour >3)
                    {
                        break;
                    }
                }
                else
                {
                    digestedRedditPosts.Add(new(post));
                }
            }
            if (posts.Count == 100 
                && posts.Last().Created > DateTime.Now.AddHours(-1))
            {
                digestedRedditPosts.AddRange(
                    GetLastHoursPosts(subReddit, posts.Last().Fullname));
            }
            return digestedRedditPosts;
        }

    }
}
