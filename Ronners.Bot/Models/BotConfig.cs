using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{    
    public class BotConfig    
    {
        [JsonPropertyName("Discord Key")]
        public string DiscordKey{get;set;}
        
        [JsonPropertyName("IEX Key")]
        public string IEXKey { get; set; }

        [JsonPropertyName("Command Prefix")]
        public char CommandPrefix {get;set;}

        [JsonPropertyName("Captchas Folder")]
        public string CaptchaFolder {get;set;}

        [JsonPropertyName("Images Folder")]
        public string ImgFolder{get;set;}

        [JsonPropertyName("Audio Clip Folder")]
        public string AudioFolder{get;set;}

        [JsonPropertyName("Allowed Image Manipulation Channels")]
        public List<ulong> WhitelistedChannel{get;set;}

        [JsonPropertyName("Database Connection String")]
        public string DatabaseConnection{get;set;}
        [JsonPropertyName("Mongo Db Connection String")]
        public string MongoConnectionString{get;set;}

        [JsonPropertyName("Ron Stock Json File")]
        public string StockJson{get;set;}

        [JsonPropertyName("Google Books API Key")]
        public string GoogleBooksKey{get;set;}
        [JsonPropertyName("JellyFin API Key")]
        public string JellyfinKey{get;set;}
        [JsonPropertyName("JellyFin User ID")]
        public string JellyFinUserID{get;set;}
        [JsonPropertyName("Lancer Content Pack")]
        public string LCP{get;set;}
        [JsonPropertyName("Fish Data Json File")]
        public string FishJson{get;set;}

    }
}
