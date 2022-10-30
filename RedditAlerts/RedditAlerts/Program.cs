using Reddit.Controllers;
using RedditAlerts.Managers;
using RedditAlerts.Models;

namespace RedditAlerts;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Reddit!");
        SettingsModel settings = SettingsManager.GettSettings();
        RedditManager redditManager = new("",
            "");
        redditManager.SearchPosts("GME", settings.SubReddits.First());
        List<DigestedRedditPost> posts = new();
        foreach(var sub in settings.SubReddits)
        {
            posts.AddRange(redditManager.GetLastHoursPosts(sub));
        }

        Console.WriteLine("done :)");
    }
}