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
        System.IO.Directory.CreateDirectory(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) +
                    "\\RedditAlets");
        string settingsStr = FileService.GetFullFile(
           Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData)+
                    "\\RedditAlets\\settings.json");
        SettingsModel settingsObj = new ();
        if(string.IsNullOrWhiteSpace(settingsStr))
        {
            Console.WriteLine("No settings found, creating default settings");
            settingsObj.KeyWords.Add("GME");
            settingsObj.SubReddits.Add("wallstreetbets");
            SaveSettings(settingsObj);
        }
        else
        {
            settingsObj = JsonSerializer.Deserialize<SettingsModel>(settingsStr) ?? new();
        }
        return settingsObj;
    }

    public static void SaveSettings(SettingsModel settingsObj)
    {
        string settingsStr = JsonSerializer.Serialize(settingsObj);
        FileService.WriteToFile(
            Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData) +
                    "\\RedditAlets\\settings.json",
            settingsStr);
    }
}