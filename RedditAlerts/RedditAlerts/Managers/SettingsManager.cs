using RedditAlerts.Models;
using RedditAlerts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedditAlerts.Managers;
public static class SettingsManager
{
    public static SettingsModel GettSettings()
    {
        System.IO.Directory.CreateDirectory("RedditAlets");
        string settingsStr = FileService.GetFullFile(
            "RedditAlets\\settings.json");
        SettingsModel settingsObj = new ();
        if(string.IsNullOrWhiteSpace(settingsStr))
        {
            Console.WriteLine("No settings found, creating default settings");
            settingsObj.KeyWords.Add("GME");
            settingsObj.SubReddits.Add("wallstreetbets");
            settingsStr = JsonSerializer.Serialize(settingsObj);
            FileService.WriteToFile("RedditAlets\\settings.json",
                settingsStr);
        }
        else
        {
            settingsObj = JsonSerializer.Deserialize<SettingsModel>(settingsStr) ?? new();
        }
        return settingsObj;
    }
}