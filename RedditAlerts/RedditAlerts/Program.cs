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
        if(args.Length == 1)
        {
            KeyVaultSecretService akv = new(args[0]);
            //this assumes you have a key vault with your refresh token and avoids the browser
            string appUserName = await akv.GetKeyVaultSecretAsync("RedditAppUser");
            string appPassword = await akv.GetKeyVaultSecretAsync("RedditAppPassword");
            string appRefreshToken = await akv.GetKeyVaultSecretAsync("RedditAppRefreshToken");
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
                redditManager.GetLastHoursPosts(sub));
        }

        Console.WriteLine("done :)");
    }
}