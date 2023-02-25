using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Ronners.Bot.Models.JellyFin
{    
  public class Item 
  {
    [JsonPropertyName("Id")]
    public string Id { get; set; }
    
    [JsonPropertyName("Name")]
    public string Name{get;set;}

    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("Album")]
    public string Album { get; set; }

    [JsonPropertyName("Artists")]
    public List<string> Artists{get;set;}

    [JsonPropertyName("RunTimeTicks")]
    public long RunTimeTicks {get;set;}
  }
}
