using System.Text.Json.Serialization;


namespace Ronners.Bot.Models.GoogleBooks
{    
  public class Volume    {

        [JsonPropertyName("selfLink")]
        public string SelfLink{get;set;}
        [JsonPropertyName ("volumeInfo")]
        public VolumeInfo VolumeInfo{get;set;}

    }
}
