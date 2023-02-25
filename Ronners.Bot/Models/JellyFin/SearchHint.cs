using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Ronners.Bot.Models.JellyFin
{    
  public class SearchHint 
  {
    [JsonPropertyName("ItemId")]
    public string ItemId {get;set;}

    [JsonPropertyName("Id")]
    public string Id { get; set; }
    
    [JsonPropertyName("Name")]
    public string Name{get;set;}

    [JsonPropertyName("AlbumArtist")]
    public string AlbumArtist { get; set; }

    [JsonPropertyName("Album")]
    public string Album { get; set; }

  }
}
