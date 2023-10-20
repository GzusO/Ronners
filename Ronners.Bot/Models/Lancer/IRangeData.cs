using System.Text.Json.Serialization;

namespace Ronners.Bot.Models.Lancer
{
    public class IRangeData
    {
        [JsonPropertyName("type")]
        public RangeType Type{get;set;}
        [JsonPropertyName("val")]
        public string Value {get;set;} 
    }
}