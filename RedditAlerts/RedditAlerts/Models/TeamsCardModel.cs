using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RedditAlerts.Models
{

    public class TeamsCardModel
    {
        public TeamsCardModel(DigestedRedditPost post)
        {
            title = post.Title;
            if(string.IsNullOrWhiteSpace(post.Content))
            {
                text = "Image post";
            }
            else
            {
                text = post.Content;
            }
            List<Fact> facts = new()
            {
                new("Subreddit", post.SubReddit),
                new("Posted Date", post.PostedDate.ToString()),
            };
            sections = new Section[1]
            {
                new(facts)
            };
            potentialAction = new Potentialaction[1]
            {
                new(post.URL)
            };
        }
        [JsonPropertyName("@context")]
        public string context { get; set; } = "https://schema.org/extensions";
        [JsonPropertyName("@type")]
        public string type { get; set; } = "MessageCard";
        [JsonPropertyName("themeColor")]
        public string themeColor { get; set; } = "0072C6";
        [JsonPropertyName("title")]
        public string title { get; set; }
        [JsonPropertyName("text")]
        public string text { get; set; } = "";
        [JsonPropertyName("sections")]
        public Section[] sections { get; set; }
        [JsonPropertyName("potentialAction")]
        public Potentialaction[] potentialAction { get; set; }
    }

    public class Section
    {
        public Section(List<Fact> facts)
        {
            this.facts = facts;
        }

        [JsonPropertyName("facts")]
        public List<Fact> facts { get; set; } = new();
        [JsonPropertyName("markdown")]
        public bool markdown { get; set; } = true;
    }

    public class Fact
    {
        public Fact(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("value")]
        public string value { get; set; }
    }

    public class Potentialaction
    {
        public Potentialaction(string url)
        {
            targets = new Target[1]
            {
                new(url)
            };
        }
        [JsonPropertyName("@type")]
        public string type { get; set; } = "OpenUri";
        [JsonPropertyName("name")]
        public string name { get; set; } = "View on Reddit";
        [JsonPropertyName("targets")]
        public Target[] targets { get; set; } = new Target[0];
    }

    public class Target
    {
        public Target(string url)
        {
            uri = url;
        }
        [JsonPropertyName("os")]
        public string os { get; set; } = "default";
        [JsonPropertyName("uri")]
        public string uri { get; set; } 
    }

}
