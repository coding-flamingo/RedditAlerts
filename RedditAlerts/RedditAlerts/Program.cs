using Reddit.Controllers;
using RedditAlerts.Managers;
using RedditAlerts.Models;
using RedditAlerts.Services;

namespace RedditAlerts;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Reddit!");
        RedditManager redditManager;
        string teamsURL = string.Empty;
        if(args.Length == 1)
        {
            KeyVaultSecretService akv = new(args[0]);
            //this assumes you have a key vault with your refresh token and avoids the browser
            string appUserName = await akv.GetKeyVaultSecretAsync("RedditAppUser");
            string appPassword = await akv.GetKeyVaultSecretAsync("RedditAppPassword");
            string appRefreshToken = await akv.GetKeyVaultSecretAsync("RedditAppRefreshToken");
            teamsURL = await akv.GetKeyVaultSecretAsync("TeamsURL");
            redditManager = new (appUserName, appPassword, appRefreshToken);
        }
        else
        {
            redditManager = new("Your App UserName",
            "Your App Secret");
        }
        SettingsModel settings = SettingsManager.GettSettings();
        
        List<DigestedRedditPost> posts = new();
        foreach(var sub in settings.SubReddits)
        {
            posts.AddRange(
                redditManager.GetLastHoursFilteredPosts(sub, settings.KeyWords));
        }
        //Send Messages to Teams
        if(!string.IsNullOrWhiteSpace(teamsURL))
        {
            TeamsManager teamsManager = new(new(new()), teamsURL);
            await teamsManager.SendNewPostsToTeamsAsync(settings.SentPosts, 
                posts);
        }
        //Save Settings
        settings.SentPosts = settings.SentPosts.Where(i => i.PostedDate 
            > DateTime.Now.AddMinutes(-100)).ToList();
        SettingsManager.SaveSettings(settings);
        Console.WriteLine("done :)");
    }
}