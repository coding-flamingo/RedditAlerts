using RedditAlerts.Models;
using RedditAlerts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditAlerts.Managers
{
    public class TeamsManager
    {
        private readonly string _url;
        private readonly HttpClientService _httpClient;
        public TeamsManager(HttpClientService httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        public async Task SendNewPostsToTeamsAsync(
            List<DigestedRedditPost> sentPosts,
            List<DigestedRedditPost> newPosts)
        {
            List<DigestedRedditPost> postsToSend = newPosts.Where(i =>
                sentPosts.Select(x => x.URL).Contains(i.URL) == false)
                .ToList();
            foreach (DigestedRedditPost post in postsToSend)
            {
                if(await SendPostAsync(new(post)))
                {
                    sentPosts.Add(post);
                }
            }
        }

        public async Task<bool> SendPostAsync(TeamsCardModel post)
        {
            var result = await _httpClient.CallPost(_url, 
                JsonSerializer.Serialize(post));
            if(result.Success == false)
            {
                Console.WriteLine("Error posting "+
                    JsonSerializer.Serialize(post) +
                    " with error " + result.Message);
            }
            return result.Success;
        }
    }
}
