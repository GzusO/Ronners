using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Ronners.Bot.Models.JellyFin
{    
  public class ItemsResult    
  {
    [JsonPropertyName("Items")]
    public List<JellyFin.Item> Items{get;set;}
    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount {get;set;}
    [JsonPropertyName("StartIndex")]
    public int StartIndex{get;set;}
  }
}
