using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Ronners.Bot.Models.JellyFin
{    
  public class SearchHintResult    
  {
    [JsonPropertyName("SearchHints")]
    public List<SearchHint> SearchHints{get;set;}
    [JsonPropertyName("TotalRecordCount")]
    public int TotalRecordCount {get;set;}
  }
}
