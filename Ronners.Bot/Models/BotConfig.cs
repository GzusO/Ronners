using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models
{    
    public class BotConfig    
    {
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

    }
}
