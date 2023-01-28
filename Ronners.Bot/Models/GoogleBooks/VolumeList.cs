using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.GoogleBooks
{    
  public class VolumeList    {
        [JsonPropertyName("kind")]
        public string Kind {get;set;}
        [JsonPropertyName("items")]
        public IList<Volume> Items{get;set;}
        [JsonPropertyName("totalItems")]
        public int Count {get;set;}

    }
}