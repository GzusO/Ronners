using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.GoogleBooks
{    
  public class VolumeInfo {
        [JsonPropertyName("title")]
        public string Title {get;set;}
        [JsonPropertyName("subtitle")]
        public string Subtitle {get;set;}
        [JsonPropertyName("authors")]
        public IList<string> Authors{get;set;}
        [JsonPropertyName("description")]
        public string Description {get;set;}
        [JsonPropertyName("publishedDate")]
        public string PublishedDate {get;set;}
        [JsonPropertyName("pageCount")]
        public int PageCount {get;set;}
        [JsonPropertyName("mainCategory")]
        public string MainCategory {get;set;}
        [JsonPropertyName("infoLink")]
        public string InfoLink{get;set;}
    }
}