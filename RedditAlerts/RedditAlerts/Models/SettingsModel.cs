using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedditAlerts.Models
{
    public class SettingsModel
    {
        [JsonPropertyName("SubReddits")]
        public List<string> SubReddits { get; set; } = new();
        [JsonPropertyName("KeyWords")]
        public List<string> KeyWords { get; set; } = new();
        [JsonPropertyName("SentPosts")]
        public List<DigestedRedditPost> SentPosts { get; set; } = new();
    }
}
